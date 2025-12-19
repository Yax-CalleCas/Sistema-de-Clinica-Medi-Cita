using Microsoft.Data.SqlClient;
using System.Data;
using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;

namespace MediCita.Web.Servicios.Implementacion
{
    public class EspecialidadService : IEspecialidadService
    {
        private readonly IConfiguration _configuration;
        
        public EspecialidadService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<List<Especialidad>> Listar()
        {
            var lista = new List<Especialidad>();
            string cadena = _configuration.GetConnectionString("CadenaSQL");

            using (SqlConnection cn = new SqlConnection(cadena))
            using (SqlCommand cmd = new SqlCommand("usp_ListarEspecialidades", cn))
            {
                cmd.CommandType = CommandType.StoredProcedure;

                try
                {
                    await cn.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        while (await dr.ReadAsync())
                        {
                            lista.Add(new Especialidad
                            {
                                IdEspecialidad = dr.GetInt32(0),
                                NombreEspec = dr.GetString(1),
                                Descripcion = dr.FieldCount > 2 && dr["Descripcion"] != DBNull.Value
                                              ? dr["Descripcion"].ToString()
                                              : null
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Error al listar especialidades: " + ex.Message, ex);
                }
            }

            return lista;
        }
    }
}
