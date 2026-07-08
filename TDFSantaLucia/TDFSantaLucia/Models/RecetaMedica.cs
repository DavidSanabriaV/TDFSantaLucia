using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class RecetaMedica
    {
        [Key]
        public int Receta_Id { get; set; }

        public string? Descripcion { get; set; }

        public string? Frecuencia { get; set; }

        public DateTime Fecha_Emision { get; set; }

        public DateTime? Fecha_Vencimiento { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey(nameof(Producto))]
        public int Producto_Id { get; set; }
        public Producto? Producto { get; set; }

        [ForeignKey(nameof(Expediente))]
        public int Expediente_Id { get; set; }
        public Expediente? Expediente { get; set; }
    }
}