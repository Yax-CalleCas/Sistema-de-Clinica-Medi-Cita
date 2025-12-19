namespace MediCita.Web.Entidades
{
    public class ReporteCita
    {
        public int IdCita { get; set; }
        public string? Paciente { get; set; }
        public string? Medico { get; set; }
        public string? Especialidad { get; set; }
        public DateTime FechaCita { get; set; }
        public TimeSpan? HoraInicio { get; set; }
        public TimeSpan? HoraFin { get; set; }
        public decimal? MontoPagar { get; set; }
        public string? Estado { get; set; }
        public string? CMP { get; set; }
        public TimeSpan HoraCita { get; set; }


        public string Consultorio { get; set; } = "";
        public string TipoCita { get; set; } = "";

    }
}