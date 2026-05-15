using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Expediente
    {
        [Key]
        public int Expediente_Id { get; set; }

        public string? Descripcion { get; set; }

        public DateTime Fecha_Creacion { get; set; }

        public DateTime Fecha_Actualizacion { get; set; }

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(Empleado))]
        public int Empleado_Id { get; set; }
        public Empleado? Empleado { get; set; }

        public List<RecetaMedica> RecetasMedicas { get; set; } = new();
    }
}