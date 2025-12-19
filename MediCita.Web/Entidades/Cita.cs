namespace MediCita.Web.Entidades
{
    public class Cita
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public int IdMedico { get; set; }
        public string? Paciente { get; set; } // Nombre para mostrar
        public string? CMP { get; set; }      // Identificador del médico
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public decimal MontoPagar { get; set; }
        public string? Estado { get; set; }   // P, A, C
        public string? Nota { get; set; }     // Nota de atención médica
    }
}