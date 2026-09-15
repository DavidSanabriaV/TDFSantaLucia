using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Enfermedad
    {
        [Key]
        public int Enfermedad_Id { get; set; }

        [Required(ErrorMessage = "La descripción de la enfermedad es obligatoria")]
        public string Descripcion { get; set; }

        [ForeignKey(nameof(Empleado))]
        public int Empleado_Id { get; set; }
        public Empleado? Empleado { get; set; }
    }
}