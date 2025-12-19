using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MediCita.Web.Entidades
{
    public class Medico : Usuario
    {
        public int IdEspecialidad { get; set; }
        public string? Especialidad { get; set; } // Nombre de la especialidad
        public string? CMP { get; set; } = string.Empty;
        public string? RNE { get; set; }
        public string? Telefono { get; set; }
        public decimal PrecioConsulta { get; set; } = 80.00m;
        public int DuracionMinutos { get; set; } = 40;
        public string? TipoServicio { get; set; } // Presencial / Virtual
    }
}
