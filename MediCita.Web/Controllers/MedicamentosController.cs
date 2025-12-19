using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using System.Text.RegularExpressions;

namespace MediCita.Web.Controllers
{
    // Restringe el acceso a todo el controlador; solo usuarios con cookie de autenticación válida pueden ingresar
    [Authorize]
    public class MedicamentosController : Controller
    {
        private readonly IMedicamentoService _servicio;

        public MedicamentosController(IMedicamentoService servicio)
        {
            _servicio = servicio;
        }

        // Recupera la lista de medicamentos aplicando una lógica de paginación manual en memoria
        public async Task<IActionResult> Index(int pagina = 1)
        {
            int registrosPorPagina = 5;

            var listaCompleta = await _servicio.Listar();

            // Cálculo para determinar la cantidad de páginas necesarias según el total de registros
            int totalRegistros = listaCompleta.Count();
            int totalPaginas = (int)Math.Ceiling(totalRegistros / (double)registrosPorPagina);

            // Aplicación de LINQ para segmentar la lista según la página solicitada
            var listaPaginada = listaCompleta
                .Skip((pagina - 1) * registrosPorPagina)
                .Take(registrosPorPagina)
                .ToList();

            // Envío de metadatos de paginación a la vista para renderizar controles de navegación
            ViewBag.PaginaActual = pagina;
            ViewBag.TotalPaginas = totalPaginas;

            return View(listaPaginada);
        }

        // Retorna el formulario de creación vacío
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken] // Protege contra ataques CSRF validando el token del formulario
        public async Task<IActionResult> Crear(Medicamento modelo)
        {
            // Regla de Negocio: La promoción es opcional, pero si existe, debe cumplir el formato "número%"
            if (!string.IsNullOrWhiteSpace(modelo.Promocion) &&
                !Regex.IsMatch(modelo.Promocion.Trim(), @"^\d{1,2}%$"))
            {
                ModelState.AddModelError("Promocion", "La promoción debe ser un porcentaje válido. Ejemplo: 10%");
            }

            // Verifica si las validaciones de DataAnnotations y las personalizadas fallaron
            if (!ModelState.IsValid)
                return View(modelo);

            bool respuesta = await _servicio.Guardar(modelo);

            if (respuesta)
                return RedirectToAction(nameof(Index));

            ViewData["Mensaje"] = "No se pudo crear el medicamento.";
            return View(modelo);
        }

        // Busca el medicamento por su ID para cargar sus datos en el formulario de edición
        public async Task<IActionResult> Editar(int id)
        {
            var modelo = await _servicio.Obtener(id);
            if (modelo == null)
                return NotFound();

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Medicamento modelo)
        {
            // Re-validación de formato de promoción en edición para asegurar integridad de datos
            if (!string.IsNullOrWhiteSpace(modelo.Promocion) &&
                !Regex.IsMatch(modelo.Promocion.Trim(), @"^\d{1,2}%$"))
            {
                ModelState.AddModelError("Promocion", "La promoción debe ser un porcentaje válido. Ejemplo: 15%");
            }

            if (!ModelState.IsValid)
                return View(modelo);

            bool respuesta = await _servicio.Editar(modelo);

            if (respuesta)
                return RedirectToAction(nameof(Index));

            ViewData["Mensaje"] = "No se pudo actualizar el medicamento.";
            return View(modelo);
        }

        // Realiza la eliminación física del registro y redirecciona a la lista principal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            await _servicio.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }
    }
}