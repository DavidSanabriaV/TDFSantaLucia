using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class CitaViewModel
    {
        public int Cita_Id { get; set; }

        [Required(ErrorMessage = "El servicio es obligatorio")]
        public string Servicio { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        public string? Observaciones { get; set; }

        public string? Estado { get; set; }

        public int Cliente_Id { get; set; }

        public int? Empleado_Id { get; set; }

        public string? NombreCliente { get; set; }
        public string? NombreEmpleado { get; set; }

        public List<Cliente> Clientes { get; set; } = new();
        public List<Empleado> Empleados { get; set; } = new();
    }
}