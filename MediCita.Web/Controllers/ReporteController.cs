using MediCita.Web.Servicios.Contrato;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MediCita.Web.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReporteController : Controller
    {
        private readonly IReporte _reporteService;
        private const int PageSize = 4;

        public ReporteController(IReporte reporteService)
        {
            _reporteService = reporteService;
        }

        // VISTA INICIAL
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // ===============================
        // REPORTE DE CITAS MÉDICAS
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteCitas(DateTime fechaInicio, DateTime fechaFin, int page = 1)
        {
            if (!RangoValido(fechaInicio, fechaFin))
            {
                TempData["Error"] = "El rango de fechas no es válido.";
                return RedirectToAction(nameof(Index));
            }

            var data = await _reporteService.ReporteCitas(fechaInicio, fechaFin);

            var resultado = data
                .OrderByDescending(x => x.FechaCita)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            CargarViewBagPaginacion(data.Count, page, fechaInicio, fechaFin);

            return View(resultado);
        }

        // ===============================
        // REPORTE DE VENTAS
        // ===============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReporteVentas(DateTime fechaInicio, DateTime fechaFin, int page = 1)
        {
            if (!RangoValido(fechaInicio, fechaFin))
            {
                TempData["Error"] = "El rango de fechas no es válido.";
                return RedirectToAction(nameof(Index));
            }

            var data = await _reporteService.ReporteVentas(fechaInicio, fechaFin);

            var resultado = data
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            CargarViewBagPaginacion(data.Count, page, fechaInicio, fechaFin);

            return View(resultado);
        }

        // ===============================
        // EXPORTAR CITAS A PDF
        // ===============================
        [HttpGet]
        public async Task<IActionResult> ExportarCitasPdf(DateTime fechaInicio, DateTime fechaFin)
        {
            if (!RangoValido(fechaInicio, fechaFin))
            {
                return RedirectToAction(nameof(Index));
            }

            var data = await _reporteService.ReporteCitas(fechaInicio, fechaFin);

            ViewBag.FechaInicio = fechaInicio.ToString("dd/MM/yyyy");
            ViewBag.FechaFin = fechaFin.ToString("dd/MM/yyyy");

            return View("ExportarCitasPdf", data);
        }

        // ===============================
        // MÉTODOS PRIVADOS
        // ===============================
        private static bool RangoValido(DateTime inicio, DateTime fin)
        {
            return inicio.Date <= fin.Date;
        }

        private void CargarViewBagPaginacion(int totalRegistros, int page, DateTime inicio, DateTime fin)
        {
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalRegistros / (double)PageSize);
            ViewBag.FechaInicio = inicio.ToString("yyyy-MM-dd");
            ViewBag.FechaFin = fin.ToString("yyyy-MM-dd");
        }
    }
}
