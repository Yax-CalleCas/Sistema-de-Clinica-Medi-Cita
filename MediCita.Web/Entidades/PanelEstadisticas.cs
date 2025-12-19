namespace MediCita.Web.Entidades
{
    public class PanelEstadisticas
    {
        public int TotalPacientes { get; set; }
        public int TotalMedicosRegistrados { get; set; }
        public int CitasHoy { get; set; }
        public decimal VentasMesActual { get; set; }
        public string TopMedicamentosJson { get; set; } = "[]";
        public int ProductosStockBajo { get; set; }
        public string MedicamentosStockBajoJson { get; set; } = "[]";

    }
}
