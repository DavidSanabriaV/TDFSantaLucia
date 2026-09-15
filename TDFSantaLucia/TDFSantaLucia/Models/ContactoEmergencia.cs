using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class ContactoEmergencia
    {
        [Key]
        public int ContactoEmergencia_Id { get; set; }

        [Required(ErrorMessage = "El nombre del contacto es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El teléfono del contacto es obligatorio")]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El teléfono debe tener exactamente 8 números")]
        public string Telefono { get; set; }

        public string? Parentesco { get; set; }

        [ForeignKey(nameof(Empleado))]
        public int Empleado_Id { get; set; }
        public Empleado? Empleado { get; set; }
    }
}