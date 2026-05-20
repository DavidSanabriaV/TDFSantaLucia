using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class ClienteViewModel
    {
        public int Cliente_Id { get; set; }
        public string? Usuario_ID { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El primer apellido es obligatorio")]
        public string Primer_Apellido { get; set; }

        [Required(ErrorMessage = "El segundo apellido es obligatorio")]
        public string Segundo_Apellido { get; set; }

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo inválido")]
        public string Email { get; set; }

        public string? Cedula { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion_Exacta { get; set; }
        public bool Estado { get; set; } = true;
        public DateTime? Fecha_Nacimiento { get; set; }
        public int Puntos_Acumulados { get; set; }
    }
}