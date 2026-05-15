using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Cita
    {
        [Key]
        public int Cita_Id { get; set; }

        public string? Servicio { get; set; }

        public DateTime Fecha { get; set; }

        public string? Estado { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(Empleado))]
        public int Empleado_Id { get; set; }
        public Empleado? Empleado { get; set; }
    }
}