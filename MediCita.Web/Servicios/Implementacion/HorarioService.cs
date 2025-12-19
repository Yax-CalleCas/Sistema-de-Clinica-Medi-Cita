using MediCita.Web.Servicios.Contrato;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace MediCita.Web.Servicios.Implementacion
{
    public class HorarioService : IHorarioService
    {
        private readonly string _cadena;

        public HorarioService(IConfiguration config)
        {
            _cadena = config.GetConnectionString("CadenaSQL")!;
        }

        public async Task GenerarHorarioMedico(
            int idMedico,
            TimeSpan horarioInicio,
            TimeSpan horarioFin,
            int duracionMinutos,
            TimeSpan? almuerzoInicio = null,
            TimeSpan? almuerzoFin = null)
        {
            // Valores por defecto para almuerzo
            almuerzoInicio ??= new TimeSpan(12, 0, 0);
            almuerzoFin ??= new TimeSpan(13, 0, 0);

            var duracion = TimeSpan.FromMinutes(duracionMinutos);
            var horarios = new List<(TimeSpan, TimeSpan)>();
            TimeSpan actual = horarioInicio;

            while (actual + duracion <= horarioFin)
            {
                // Saltar la hora de almuerzo
                if (actual >= almuerzoInicio && actual < almuerzoFin)
                {
                    actual = almuerzoFin.Value;
                    continue;
                }

                horarios.Add((actual, actual + duracion));
                actual += duracion;
            }

            // Insertar en BD
            await using var con = new SqlConnection(_cadena);
            await con.OpenAsync();

            foreach (var h in horarios)
            {
                using var cmd = new SqlCommand("usp_InsertarHorarioMedico", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@IdMedico", idMedico);
                cmd.Parameters.AddWithValue("@HoraInicio", h.Item1);
                cmd.Parameters.AddWithValue("@HoraFin", h.Item2);

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
