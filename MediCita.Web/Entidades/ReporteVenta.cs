namespace MediCita.Web.Entidades
{
    public class ReporteVenta
    {
        public int IdVenta { get; set; }
        public string Paciente { get; set; } = "";
        public DateTime FechaVenta { get; set; }

        public string NombreMedicamento { get; set; } = "";
        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
        public decimal SubTotal { get; set; }

        public decimal VentaSubTotal { get; set; }
        public decimal IGV { get; set; }
        public decimal TotalFinal { get; set; }

        public string MetodoPago { get; set; } = "";
        public int PorcentajeDescuento { get; set; }
    }

}
