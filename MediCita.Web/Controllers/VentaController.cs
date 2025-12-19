using MediCita.Web.Entidades;
using MediCita.Web.Servicios.Contrato;
using MediCita.Web.Utilidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MediCita.Web.Controllers
{
    [Authorize(Roles = "Paciente,Admin")]
    public class VentaController : Controller
    {
        private readonly IMedicamentoService _medicamentos;
        private readonly IVentaService _ventas;

        public VentaController(IMedicamentoService medicamentos, IVentaService ventas)
        {
            _medicamentos = medicamentos;
            _ventas = ventas;
        }

        public async Task<IActionResult> Catalogo(string buscar, string categoria)
        {
            var lista = await _medicamentos.Listar();

            if (!string.IsNullOrEmpty(buscar))
                lista = lista.Where(x => x.Nombre.Contains(buscar, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrEmpty(categoria))
                lista = lista.Where(x => x.Categoria != null && x.Categoria.Equals(categoria, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.ContadorCarrito = CarritoSession.Sum(x => x.Cantidad);
            return View(lista);
        }

        [HttpPost]
        public async Task<IActionResult> AgregarCarrito(int idMedicamento, int cantidad = 1)
        {
            var p = await _medicamentos.Obtener(idMedicamento);
            if (p == null) return NotFound();

            var carrito = CarritoSession;
            var item = carrito.FirstOrDefault(x => x.IdMedicamento == idMedicamento);

            if (item == null)
            {
                item = new DetalleVenta
                {
                    IdMedicamento = idMedicamento,
                    NombreMedicamento = p.Nombre,
                    PrecioUnitario = p.Precio,
                    Cantidad = 0,
                    // CORRECCIÓN: Asignamos ImagenUrl y Promoción desde el inicio
                    ImagenUrl = p.ImagenUrl ?? "",
                    Promocion = p.Promocion ?? ""
                };
                carrito.Add(item);
            }

            item.Cantidad += cantidad;

            // Actualizamos montos (Esto calcula el descuento e importe final)
            ActualizarMontosItem(item, p.Promocion);

            GuardarCarrito(carrito);

            // Sincronización con base de datos (Carrito Persistente)
            await _ventas.SincronizarCarrito(ObtenerUsuarioId(), idMedicamento, item.Cantidad);

            return RedirectToAction(nameof(Catalogo));
        }

        [HttpPost]
        public async Task<IActionResult> Eliminar(int idMedicamento)
        {
            var carrito = CarritoSession;
            var item = carrito.FirstOrDefault(x => x.IdMedicamento == idMedicamento);

            if (item != null)
            {
                carrito.Remove(item);
                await _ventas.QuitarDelCarrito(ObtenerUsuarioId(), idMedicamento);
                GuardarCarrito(carrito);
            }
            return RedirectToAction(nameof(Carrito));
        }

        [HttpPost]
        public async Task<IActionResult> ActualizarCantidad(int idMedicamento, int cantidad)
        {
            var p = await _medicamentos.Obtener(idMedicamento);
            var carrito = CarritoSession;
            var item = carrito.FirstOrDefault(x => x.IdMedicamento == idMedicamento);

            // Validamos stock si es necesario
            if (item != null && p != null && cantidad > 0)
            {
                item.Cantidad = cantidad;
                ActualizarMontosItem(item, p.Promocion);
                GuardarCarrito(carrito);
                await _ventas.SincronizarCarrito(ObtenerUsuarioId(), idMedicamento, cantidad);
            }
            return RedirectToAction(nameof(Carrito));
        }

        public IActionResult Carrito() => View(CarritoSession);

        public IActionResult ConfirmarCompra()
        {
            var detalles = CarritoSession;
            if (!detalles.Any()) return RedirectToAction(nameof(Catalogo));

            var venta = new Venta
            {
                Detalles = detalles,
                Total = detalles.Sum(x => x.Importe)
            };

            // Cálculos contables para la vista de confirmación
            venta.SubTotal = venta.Total / 1.18m;
            venta.IGV = venta.Total - venta.SubTotal;

            return View(venta);
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarVenta(string metodoPago)
        {
            var detalles = CarritoSession;
            if (!detalles.Any()) return RedirectToAction(nameof(Catalogo));

            var venta = new Venta
            {
                IdUsuario = ObtenerUsuarioId(),
                MetodoPago = metodoPago ?? "EFECTIVO",
                Detalles = detalles,
                Total = detalles.Sum(x => x.Importe),
                FechaVenta = DateTime.Now
            };

            try
            {
                // El Service se encarga del TVP de 6 columnas y del Procedimiento Almacenado
                var r = await _ventas.Registrar(venta);
                if (r != null && r.IdVenta > 0)
                {
                    // Limpiamos la sesión tras éxito
                    HttpContext.Session.Remove("CarritoCompra");
                    return RedirectToAction(nameof(DetalleVenta), new { id = r.IdVenta });
                }
            }
            catch (Exception ex)
            {
                // Captura errores de SQL, TVP o lógica de negocio
                TempData["ErrorVenta"] = "No se pudo completar el pedido: " + ex.Message;
            }

            return RedirectToAction(nameof(ConfirmarCompra));
        }

        public async Task<IActionResult> DetalleVenta(int id)
        {
            try
            {
                var venta = await _ventas.ObtenerPorId(id);
                return View(venta);
            }
            catch
            {
                return RedirectToAction(nameof(Catalogo));
            }
        }

        // --- MÉTODOS AUXILIARES ---

        private void ActualizarMontosItem(DetalleVenta item, string promocion)
        {
            item.Promocion = promocion ?? "";

            decimal porcentaje = 0;
            if (!string.IsNullOrEmpty(promocion) && promocion.Contains("%"))
            {
                // Extrae el número del string "10%" -> 10
                string soloNumero = new string(promocion.Where(char.IsDigit).ToArray());
                decimal.TryParse(soloNumero, out porcentaje);
            }

            decimal precioConDescuento = item.PrecioUnitario * (1 - (porcentaje / 100));
            item.Descuento = (item.PrecioUnitario - precioConDescuento) * item.Cantidad;
            item.Importe = precioConDescuento * item.Cantidad;
        }

        private int ObtenerUsuarioId()
        {
            var claimId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimId, out int id) ? id : 0;
        }

        private List<DetalleVenta> CarritoSession =>
            HttpContext.Session.GetObject<List<DetalleVenta>>("CarritoCompra") ?? new List<DetalleVenta>();

        private void GuardarCarrito(List<DetalleVenta> carrito) =>
            HttpContext.Session.SetObject("CarritoCompra", carrito);
    }
}