using System.Collections.Generic;

namespace MediCita.Web.Entidades
{
    public class Venta
    {
        public int IdVenta { get; set; }
        public int IdUsuario { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal SubTotal { get; set; }
        public decimal IGV { get; set; }
        public decimal Total { get; set; }
        public string MetodoPago { get; set; } = "EFECTIVO";
        public List<DetalleVenta> Detalles { get; set; } = new();
    }
}
