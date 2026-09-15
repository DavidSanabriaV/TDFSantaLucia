using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Alergia
    {
        [Key]
        public int Alergia_Id { get; set; }

        [Required(ErrorMessage = "La descripción de la alergia es obligatoria")]
        public string Descripcion { get; set; }

        [ForeignKey(nameof(Empleado))]
        public int Empleado_Id { get; set; }
        public Empleado? Empleado { get; set; }
    }
}