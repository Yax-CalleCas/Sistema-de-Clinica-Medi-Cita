using MediCita.Web.Servicios;
using MediCita.Web.Servicios.Contrato;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace MediCita.Web.Controllers
{
    // Restringe el acceso exclusivo a usuarios con el rol de Administrador para proteger el Dashboard
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly IAdminUsuariosService _adminService;
        private readonly IAdminEstadisticas _estadisticasService;

        public AdminController(
            IAdminUsuariosService adminService,
            IAdminEstadisticas estadisticasService)
        {
            _adminService = adminService;
            _estadisticasService = estadisticasService;
        }

        // Renderiza la vista principal del panel de administración
        public IActionResult Dashboard()
        {
            return View();
        }

        // Proporciona datos asíncronos en formato JSON para la construcción de gráficos y contadores
        [HttpGet]
        public async Task<IActionResult> ObtenerEstadisticas()
        {
            try
            {
                // Recupera el objeto consolidado de estadísticas desde la capa de servicios
                var estadisticas = await _estadisticasService.ObtenerEstadisticas();

                // Lógica de procesamiento: SQL Server devuelve resultados complejos (como el Top 5) en formato String JSON.
                // Se deserializan a objetos de C# para que el método Json() de ASP.NET los envíe de forma nativa al cliente.
                var topMedicamentos = JsonSerializer.Deserialize<object>(estadisticas.TopMedicamentosJson ?? "[]");
                var stockBajoList = JsonSerializer.Deserialize<object>(estadisticas.MedicamentosStockBajoJson ?? "[]");

                // Retorna un objeto anónimo estructurado para facilitar el consumo desde JavaScript (Fetch/AJAX)
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        estadisticas.TotalPacientes,
                        estadisticas.TotalMedicosRegistrados,
                        estadisticas.CitasHoy,
                        estadisticas.VentasMesActual,
                        topMedicamentos,
                        estadisticas.ProductosStockBajo,
                        stockBajoList
                    }
                });
            }
            catch (Exception ex)
            {
                // En caso de error en el procedimiento o conexión, se retorna un código 500 para control de excepciones en el frontend
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno al obtener estadísticas: " + ex.Message
                });
            }
        }
    }
}