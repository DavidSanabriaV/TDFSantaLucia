using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class MovimientoPuntos
    {
        [Key]
        public int Movimiento_Id { get; set; }

        public int Puntos { get; set; }             

        public string Tipo { get; set; } = string.Empty; 

        public string? Descripcion { get; set; }

        public DateTime Fecha { get; set; }

        public DateTime Fecha_Vencimiento { get; set; } 

        public bool Vencido { get; set; }

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(Pedido))]
        public int? Pedido_Id { get; set; }
        public Pedido? Pedido { get; set; }
    }
}