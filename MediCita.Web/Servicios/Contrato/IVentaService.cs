using MediCita.Web.Entidades;

namespace MediCita.Web.Servicios.Contrato
{
    public interface IVentaService
    {
        // Registro de la venta final y procesamiento de pago
        Task<Venta> Registrar(Venta modelo);

        // Recupera una venta específica con sus detalles (para la vista de éxito/ticket)
        Task<Venta> ObtenerPorId(int idVenta);

        // Persistencia del carrito en la base de datos (mientras el usuario navega)
        Task<bool> SincronizarCarrito(int idUsuario, int idMedicamento, int cantidad);

        // Elimina un producto específico del carrito en la base de datos
        Task<bool> QuitarDelCarrito(int idUsuario, int idMedicamento);

        // Recupera todos los productos guardados en el carrito del usuario
        Task<List<DetalleVenta>> ObtenerCarritoPersistente(int idUsuario);
    }
}