using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Implementacion
{
    public class CitaService : ICitaService
    {
        private readonly string cadena;

        public CitaService(IConfiguration config)
        {
            cadena = config.GetConnectionString("CadenaSQL")
                    ?? throw new Exception("CadenaSQL no encontrada en appsettings.json");
        }

        // Helper para validar existencia de columnas en el DataReader
        private bool TieneColumna(SqlDataReader dr, string nombreColumna)
        {
            for (int i = 0; i < dr.FieldCount; i++)
                if (dr.GetName(i).Equals(nombreColumna, StringComparison.OrdinalIgnoreCase))
                    return true;
            return false;
        }

        // 1. Listar Médicos: Ahora usa la entidad consolidada Medico (que hereda de Usuario)
        public async Task<List<Medico>> ListarMedicosDisponibles(int? idEspecialidad = null, DateTime? fecha = null)
        {
            var lista = new List<Medico>();

            using (SqlConnection cn = new SqlConnection(cadena))
            using (SqlCommand cmd = new SqlCommand("usp_ListarMedicosDisponibles", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdEspecialidad", idEspecialidad ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Fecha", fecha?.Date ?? (object)DBNull.Value);

                await cn.OpenAsync();
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new Medico
                        {
                            IdMedico = Convert.ToInt32(dr["IdMedico"]),
                            NombreCompleto = dr["NombreMedico"]?.ToString() ?? "",
                            CMP = dr["CMP"]?.ToString() ?? "",
                            Especialidad = dr["NombreEspec"]?.ToString() ?? "",
                            PrecioConsulta = Convert.ToDecimal(dr["PrecioConsulta"]),
                            TipoServicio = dr["TipoServicio"]?.ToString() ?? ""
                        });
                    }
                }
            }
            return lista;
        }

        // 2. Listar Citas: Usa la entidad Cita que centraliza datos de Paciente y Médico
        public async Task<List<Cita>> ListarCitas(int? idPaciente, int? idMedico)
        {
            var lista = new List<Cita>();

            using (SqlConnection cn = new SqlConnection(cadena))
            using (SqlCommand cmd = new SqlCommand("sp_ListarCitas", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                // El SP parece esperar un Id y un Rol para filtrar
                cmd.Parameters.AddWithValue("@IdUsuario", idPaciente ?? idMedico);
                cmd.Parameters.AddWithValue("@Rol", idPaciente.HasValue ? "Paciente" : "Medico");

                await cn.OpenAsync();
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new Cita
                        {
                            IdCita = Convert.ToInt32(dr["IdCita"]),
                            FechaCita = Convert.ToDateTime(dr["FechaCita"]),
                            HoraInicio = (TimeSpan)dr["HoraInicio"],
                            HoraFin = (TimeSpan)dr["HoraFin"],
                            MontoPagar = Convert.ToDecimal(dr["MontoPagar"]),
                            Estado = dr["Estado"]?.ToString() ?? "",
                            // Estos campos son opcionales dependiendo de si el SP devuelve el JOIN
                            Paciente = TieneColumna(dr, "Paciente") ? dr["Paciente"]?.ToString() : null,
                            CMP = TieneColumna(dr, "CMP") ? dr["CMP"]?.ToString() : null,
                            Nota = TieneColumna(dr, "Nota") ? dr["Nota"]?.ToString() : null
                        });
                    }
                }
            }
            return lista;
        }

        // 3. Crear Cita: Mantiene la lógica de inserción atómica
        public async Task<int> CrearCita(int idPaciente, int idMedico, DateTime fecha, TimeSpan horaInicio, TimeSpan horaFin, decimal monto)
        {
            using (SqlConnection cn = new SqlConnection(cadena))
            using (SqlCommand cmd = new SqlCommand("sp_RegistrarCita", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdPaciente", idPaciente);
                cmd.Parameters.AddWithValue("@IdMedico", idMedico);
                cmd.Parameters.AddWithValue("@Fecha", fecha.Date);
                cmd.Parameters.AddWithValue("@HoraInicio", horaInicio);
                cmd.Parameters.AddWithValue("@HoraFin", horaFin);
                cmd.Parameters.AddWithValue("@Monto", monto);

                await cn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }

        // 4. Atender Cita: Actualiza Estado y Nota en la misma tabla
        public async Task<bool> AtenderCita(int idCita, string estado, string nota = null)
        {
            using (SqlConnection cn = new SqlConnection(cadena))
            using (SqlCommand cmd = new SqlCommand("sp_AtenderCita", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdCita", idCita);
                cmd.Parameters.AddWithValue("@Estado", estado);
                cmd.Parameters.AddWithValue("@Nota", (object)nota ?? DBNull.Value);

                await cn.OpenAsync();
                int filasAfectadas = await cmd.ExecuteNonQueryAsync();
                return filasAfectadas > 0;
            }
        }

        public async Task<int> CrearCitaConPago(int idPaciente, int idMedico, DateTime fecha,
                                        TimeSpan horaInicio, TimeSpan horaFin,
                                        decimal monto, string idTransaccion)
        {
            using (SqlConnection cn = new SqlConnection(cadena))
            using (SqlCommand cmd = new SqlCommand("usp_RegistrarCitaConPago", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdPaciente", idPaciente);
                cmd.Parameters.AddWithValue("@IdMedico", idMedico);
                cmd.Parameters.AddWithValue("@Fecha", fecha.Date);
                cmd.Parameters.AddWithValue("@HoraInicio", horaInicio);
                cmd.Parameters.AddWithValue("@HoraFin", horaFin);
                cmd.Parameters.AddWithValue("@Monto", monto);
                cmd.Parameters.AddWithValue("@IdTransaccion", idTransaccion);

                await cn.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();

                int idGenerado = Convert.ToInt32(result);

                if (idGenerado == -1)
                    throw new Exception("El horario ya no se encuentra disponible.");

                return idGenerado;
            }
        }
        // 5. Horarios Disponibles: Usa la entidad HorarioMedico simplificada
        public async Task<List<HorarioMedico>> ListarHorariosDisponibles(int idMedico, DateTime fecha)
        {
            var lista = new List<HorarioMedico>();

            using (SqlConnection cn = new SqlConnection(cadena))
            using (SqlCommand cmd = new SqlCommand("sp_ListarHorariosDisponibles", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@IdMedico", idMedico);
                cmd.Parameters.AddWithValue("@Fecha", fecha.Date);

                await cn.OpenAsync();
                using (var dr = await cmd.ExecuteReaderAsync())
                {
                    while (await dr.ReadAsync())
                    {
                        lista.Add(new HorarioMedico
                        {
                            IdHorario = Convert.ToInt32(dr["IdHorario"]),
                            IdMedico = idMedico,
                            Fecha = fecha.Date,
                            HoraInicio = (TimeSpan)dr["HoraInicio"],
                            HoraFin = (TimeSpan)dr["HoraFin"],
                            Disponible = true
                        });
                    }
                }
            }
            return lista;
        }
    }
}
