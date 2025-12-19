namespace MediCita.Web.Entidades
{
    public class DetalleVenta
    {
        public int IdMedicamento { get; set; }
        public string NombreMedicamento { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Descuento { get; set; } // Valor calculado en soles
        public decimal Importe { get; set; }   // (Precio * Cantidad) - Descuento
        public string? Promocion { get; set; }
        public string? ImagenUrl { get; set; }
    }
}
