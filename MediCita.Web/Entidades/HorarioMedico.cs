using System;

namespace MediCita.Web.Entidades
{
   
    // Representa un bloque de tiempo en la agenda
    public class HorarioMedico
    {
        public int IdHorario { get; set; }
        public int IdMedico { get; set; }
        public DateTime Fecha { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
        public bool Disponible { get; set; } = true;
    }

}
