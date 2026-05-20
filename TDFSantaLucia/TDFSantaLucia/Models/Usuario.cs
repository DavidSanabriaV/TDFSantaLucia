using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace TDFSantaLucia.Models
{
    public class Usuario : IdentityUser
    {
        [Required]
        public string Nombre { get; set; }

        [Required]
        public string Primer_Apellido { get; set; }

        [Required]
        public string Segundo_Apellido { get; set; }

        [Required]
        public bool Estado { get; set; } = true;

        public string? Direccion_Exacta { get; set; }

        public string? Telefono { get; set; }

        public string? Cedula { get; set; }

        public string? Correo { get; set; }

        [NotMapped]
        public string? RolNombre { get; set; }

        public Empleado? Empleado { get; set; }
        public Cliente? Cliente { get; set; }
    }
}