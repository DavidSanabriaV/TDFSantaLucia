using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Pedido
    {
        [Key]
        public int Pedido_Id { get; set; }

        public string? Estado { get; set; }

        public decimal Total { get; set; }

        public string? Descripcion { get; set; }

        public DateTime Fecha_Creacion { get; set; }

        public DateTime Fecha_Actualizacion { get; set; }

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(Cupon))]
        public int? Cupon_Id { get; set; }
        public Cupon? Cupon { get; set; }

        public List<DetallePedido> DetallesPedido { get; set; } = new();
        public List<Factura> Facturas { get; set; } = new();
    }
}