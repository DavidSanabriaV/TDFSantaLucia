using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class DetallePedido
    {
        [Key]
        public int Detalle_Pedido_Id { get; set; }

        public int Cantidad { get; set; }

        public decimal Precio_Unitario { get; set; }

        public decimal Subtotal { get; set; }

        [ForeignKey(nameof(Pedido))]
        public int Pedido_Id { get; set; }
        public Pedido? Pedido { get; set; }

        [ForeignKey(nameof(Producto))]
        public int Producto_Id { get; set; }
        public Producto? Producto { get; set; }
    }
}