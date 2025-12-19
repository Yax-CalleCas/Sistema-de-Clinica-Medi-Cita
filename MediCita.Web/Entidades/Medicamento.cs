using System.ComponentModel.DataAnnotations;

namespace MediCita.Web.Entidades
{
    public class Medicamento
    {
        public int IdMedicamento { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;
        public string Laboratorio { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int ? Stock { get; set; }
        public string? ImagenUrl { get; set; }
        public string? Promocion { get; set; } // Ejemplo: "10%"
        public string? Categoria { get; set; }
        public string ? Descripcion { get; set; }   
    }
}
