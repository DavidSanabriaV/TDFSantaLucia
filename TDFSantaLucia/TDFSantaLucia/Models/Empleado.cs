using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Empleado
    {
        [Key]
        public int Empleado_Id { get; set; }

        [Required]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "La cédula debe tener exactamente 9 dígitos numéricos.")]
        public string Cedula { get; set; }

        public string? Puesto { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion_Exacta { get; set; }

        public bool Estado { get; set; }

        public decimal? SalarioNeto { get; set; }

        public decimal? SalarioBruto { get; set; }


        [ForeignKey(nameof(Usuario))]
        public string Usuario_ID { get; set; }
        public Usuario? Usuario { get; set; }

        public List<HorarioEmpleado> Horarios { get; set; } = new();
        public List<Cita> Citas { get; set; } = new();
        public List<Expediente> Expedientes { get; set; } = new();
    }
}