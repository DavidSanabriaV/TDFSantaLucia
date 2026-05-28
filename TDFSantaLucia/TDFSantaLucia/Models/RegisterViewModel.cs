using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        public string Primer_Apellido { get; set; }

        [Required(ErrorMessage = "El segundo apellido es obligatorio")]
        public string Segundo_Apellido { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(8, ErrorMessage = "Mínimo 8 caracteres")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirme la contraseña")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        [DataType(DataType.Password)]
        public string ConfirmarPassword { get; set; }

        public string? Cedula { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion_Exacta { get; set; }
        public DateTime? Fecha_Nacimiento { get; set; }
    }
}