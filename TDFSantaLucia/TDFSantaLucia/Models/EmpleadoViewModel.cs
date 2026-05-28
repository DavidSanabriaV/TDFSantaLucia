using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class EmpleadoViewModel
    {
        public int Empleado_Id { get; set; }

        public string? UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        public string Primer_Apellido { get; set; }

        [Required(ErrorMessage = "El segundo apellido es obligatorio")]
        public string Segundo_Apellido { get; set; }

        public string? UserName { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; }

        public string? password { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un rol")]
        public string rol { get; set; }

        [Required(ErrorMessage = "La cédula es obligatoria")]
        [RegularExpression(@"^\d{9}$",
            ErrorMessage = "La cédula debe tener exactamente 9 números")]
        public string Cedula { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [RegularExpression(@"^\d{8}$",
            ErrorMessage = "El teléfono debe tener exactamente 8 números")]
        public string? Telefono { get; set; }

        public string? Direccion_Exacta { get; set; }

        public string? Puesto { get; set; }

        public decimal? SalarioBruto { get; set; }

        public decimal? SalarioNeto { get; set; }

        public bool Estado { get; set; } = true;
    }
}