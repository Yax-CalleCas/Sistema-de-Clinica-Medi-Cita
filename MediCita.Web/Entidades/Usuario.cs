namespace MediCita.Web.Entidades
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? DNI { get; set; }
        public string Correo { get; set; } = string.Empty;
        public string Clave { get; set; } = string.Empty;
        public int IdRol { get; set; }          // 1=Admin, 2=Médico, 3=Paciente
        public bool Activo { get; set; } = true;
        public DateTime FechaRegistro { get; set; }
        public string NombreRol { get; set; } = string.Empty; // viene del join
       
        public int  IdMedico { get; set; }

    }
}

