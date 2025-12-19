using Microsoft.Data.SqlClient;
using System.Data;
using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato; // Asegúrate de que apunte a tu interfaz

namespace MediCita.Web.Servicios.Implementacion
{
    public class AdminService : IAdminEstadisticas
    {
        private readonly string _cadenaSQL;

        public AdminService(IConfiguration config)
        {
            // Almacenamos la cadena desde el constructor para mayor limpieza
            _cadenaSQL = config.GetConnectionString("CadenaSQL")!;
        }

        public async Task<PanelEstadisticas> ObtenerEstadisticas()
        {
            var modelo = new PanelEstadisticas();

            try
            {
                using (var cn = new SqlConnection(_cadenaSQL))
                {
                    using (var cmd = new SqlCommand("usp_ObtenerEstadisticasDashboard", cn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        await cn.OpenAsync();

                        using (var dr = await cmd.ExecuteReaderAsync())
                        {
                            if (await dr.ReadAsync())
                            {
                                // Usamos nombres de columnas que coincidan exactamente con el Procedure
                                modelo.TotalPacientes = Convert.ToInt32(dr["TotalPacientes"]);
                                modelo.TotalMedicosRegistrados = Convert.ToInt32(dr["TotalMedicosRegistrados"]);
                                modelo.CitasHoy = Convert.ToInt32(dr["CitasHoy"]);
                                modelo.VentasMesActual = Convert.ToDecimal(dr["VentasMesActual"]);

                                // Manejo de strings JSON para evitar nulos
                                modelo.TopMedicamentosJson = dr["TopMedicamentosJson"] != DBNull.Value
                                    ? dr["TopMedicamentosJson"].ToString()!
                                    : "[]";

                                modelo.ProductosStockBajo = Convert.ToInt32(dr["ProductosStockBajo"]);

                                modelo.MedicamentosStockBajoJson = dr["MedicamentosStockBajoJson"] != DBNull.Value
                                    ? dr["MedicamentosStockBajoJson"].ToString()!
                                    : "[]";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Aquí podrías loguear el error si tienes un Logger
                // Por ahora, devolvemos el modelo vacío con inicializadores seguros
                modelo.TopMedicamentosJson = "[]";
                modelo.MedicamentosStockBajoJson = "[]";
                Console.WriteLine("Error en Estadísticas: " + ex.Message);
            }

            return modelo;
        }
    }
}