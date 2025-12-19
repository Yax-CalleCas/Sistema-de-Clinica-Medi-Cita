using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace MediCita.Web.Servicios.Implementacion
{
    public class AdminUsuariosService : IAdminUsuariosService
    {
        private readonly string _cadena;

        public AdminUsuariosService(IConfiguration configuration)
        {
            _cadena = configuration.GetConnectionString("CadenaSQL")!;
        }

        // 1. Listar especialidades
        public async Task<List<Especialidad>> ListarEspecialidades()
        {
            var lista = new List<Especialidad>();
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_ListarEspecialidades", con) { CommandType = CommandType.StoredProcedure };
            await con.OpenAsync();

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new Especialidad
                {
                    IdEspecialidad = dr.GetInt32("IdEspecialidad"),
                    NombreEspec = dr.GetString("NombreEspec")
                });
            }
            return lista;
        }

        // 2. Listar usuarios
        public async Task<List<Usuario>> ListarUsuarios()
        {
            var lista = new List<Usuario>();
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_ListarUsuarios", con) { CommandType = CommandType.StoredProcedure };
            await con.OpenAsync();

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new Usuario
                {
                    IdUsuario = dr.GetInt32("IdUsuario"),
                    NombreCompleto = dr.GetString("NombreCompleto"),
                    DNI = dr.IsDBNull("DNI") ? null : dr.GetString("DNI"),
                    Correo = dr.GetString("Correo"),
                    Activo = dr.GetBoolean("Activo"),
                    FechaRegistro = dr.GetDateTime("FechaRegistro"),
                    NombreRol = dr.GetString("NombreRol")
                });
            }
            return lista;
        }

        // 3. Listar médicos
        public async Task<List<Medico>> ListarMedicos()
        {
            var lista = new List<Medico>();
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_ListarMedicos", con) { CommandType = CommandType.StoredProcedure };
            await con.OpenAsync();

            await using var dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                lista.Add(new Medico
                {
                    IdMedico = dr.GetInt32("IdMedico"),
                    NombreCompleto = dr.GetString("NombreCompleto"),
                    Correo = dr.GetString("Correo"),
                    Especialidad = dr.GetString("Especialidad"),
                    CMP = dr.GetString("CMP"),
                    Telefono = dr.IsDBNull("Telefono") ? null : dr.GetString("Telefono"),
                    PrecioConsulta = dr.GetDecimal("PrecioConsulta"),
                    Activo = dr.GetBoolean("Activo")
                });
            }
            return lista;
        }

        // 4. Crear paciente
        public async Task<int> CrearPaciente(string nombre, string? dni, string correo, string clave, bool activo = true)
        {
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_AdminCrearPaciente", con) { CommandType = CommandType.StoredProcedure };
            cmd.Parameters.AddWithValue("@NombreCompleto", nombre);
            cmd.Parameters.AddWithValue("@DNI", (object?)dni ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@Clave", UsuarioService.HashPassword(clave));
            cmd.Parameters.AddWithValue("@Activo", activo);

            await con.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }
        // 5. Crear médico
        public async Task<int> CrearMedico(
            string nombreCompleto,
            string? dni,
            string correo,
            string clave,
            int idEspecialidad,
            string cmp,
            string? rne,
            string? telefono,
            decimal precioConsulta,
            int duracionMinutos,
            string? tipoServicio,
            bool activo
        )
        {
            await using var cn = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_AdminCrearMedico", cn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@NombreCompleto", nombreCompleto);
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@Clave", UsuarioService.HashPassword(clave));
            cmd.Parameters.AddWithValue("@IdEspecialidad", idEspecialidad);
            cmd.Parameters.AddWithValue("@CMP", cmp);
            cmd.Parameters.AddWithValue("@PrecioConsulta", precioConsulta);
            cmd.Parameters.AddWithValue("@DuracionMinutos", duracionMinutos);

            cmd.Parameters.AddWithValue("@DNI", (object?)dni ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@RNE", (object?)rne ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telefono", (object?)telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@TipoServicio", (object?)tipoServicio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Activo", activo);

            await cn.OpenAsync();
            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // 7. Actualizar médico
        public async Task<int> ActualizarMedico(
            int idMedico,
            string nombreCompleto,
            string? dni,
            string correo,
            int idEspecialidad,
            string cmp,
            string? rne,
            string? telefono,
            decimal precioConsulta,
            int duracionMinutos,
            string? tipoServicio,
            bool activo
        )
        {
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_AdminEditarMedico", con)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.AddWithValue("@IdMedico", idMedico);
            cmd.Parameters.AddWithValue("@NombreCompleto", nombreCompleto);
            cmd.Parameters.AddWithValue("@DNI", (object?)dni ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@IdEspecialidad", idEspecialidad);
            cmd.Parameters.AddWithValue("@CMP", cmp);
            cmd.Parameters.AddWithValue("@RNE", (object?)rne ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Telefono", (object?)telefono ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@PrecioConsulta", precioConsulta);
            cmd.Parameters.AddWithValue("@DuracionMinutos", duracionMinutos);
            cmd.Parameters.AddWithValue("@TipoServicio", (object?)tipoServicio ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Activo", activo);

            await con.OpenAsync();
            object? result = await cmd.ExecuteScalarAsync();
            return Convert.ToInt32(result); // -1 = correo duplicado, -2 = error, 1 = OK
        }


        // 6. Editar paciente
        public async Task<bool> EditarPaciente(int idUsuario, string nombre, string? dni, string correo, bool activo)
        {
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_AdminEditarUsuario", con) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@NombreCompleto", nombre);
            cmd.Parameters.AddWithValue("@DNI", (object?)dni ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@Activo", activo);

            await con.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }

        
        public async Task<Medico?> ObtenerMedicoPorId(int idMedico)
        {
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_ObtenerMedicoPorId", con)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@IdMedico", idMedico);

            await con.OpenAsync();

            await using var dr = await cmd.ExecuteReaderAsync();
            if (await dr.ReadAsync())
            {
                return new Medico
                {
                    IdMedico = dr.GetInt32(dr.GetOrdinal("IdMedico")),
                    NombreCompleto = dr.GetString(dr.GetOrdinal("NombreCompleto")),
                    DNI = dr.IsDBNull(dr.GetOrdinal("DNI")) ? null : dr.GetString(dr.GetOrdinal("DNI")),
                    Correo = dr.GetString(dr.GetOrdinal("Correo")),
                    IdEspecialidad = dr.GetInt32(dr.GetOrdinal("IdEspecialidad")),
                    CMP = dr.GetString(dr.GetOrdinal("CMP")),
                    RNE = dr.IsDBNull(dr.GetOrdinal("RNE")) ? null : dr.GetString(dr.GetOrdinal("RNE")),
                    Telefono = dr.IsDBNull(dr.GetOrdinal("Telefono")) ? null : dr.GetString(dr.GetOrdinal("Telefono")),
                    PrecioConsulta = dr.GetDecimal(dr.GetOrdinal("PrecioConsulta")),
                    DuracionMinutos = dr.GetInt32(dr.GetOrdinal("DuracionMinutos")),
                    TipoServicio = dr.IsDBNull(dr.GetOrdinal("TipoServicio")) ? null : dr.GetString(dr.GetOrdinal("TipoServicio")),
                    Activo = dr.GetBoolean(dr.GetOrdinal("Activo"))
                };
            }

            return null;
        }
        public async Task<string?> ValidarDatosMedicoProcedure(string dni, string correo, string cmp, int? idMedico = null)
        {
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_ValidarDatosMedico", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.AddWithValue("@DNI", (object?)dni ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Correo", correo);
            cmd.Parameters.AddWithValue("@CMP", cmp);
            cmd.Parameters.AddWithValue("@IdMedico", (object?)idMedico ?? DBNull.Value);

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString(); // Devuelve el texto del error o null si es válido
        }

        // 8. Cambiar clave
        public async Task<bool> CambiarClave(int idUsuario, string nuevaClave)
        {
            await using var con = new SqlConnection(_cadena);
            await using var cmd = new SqlCommand("usp_AdminCambiarClaveUsuario", con) { CommandType = CommandType.StoredProcedure };

            cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
            cmd.Parameters.AddWithValue("@NuevaClave", UsuarioService.HashPassword(nuevaClave));

            await con.OpenAsync();
            return Convert.ToInt32(await cmd.ExecuteScalarAsync()) > 0;
        }
    }
}
