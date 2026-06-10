using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Factura
    {
        [Key]
        public int Factura_Id { get; set; }

        public string? Numero_Factura { get; set; }

        public decimal Subtotal { get; set; }

        public decimal Descuento { get; set; }

        public decimal Impuesto { get; set; }

        public decimal Total { get; set; }

        public string? Estado { get; set; }

        public DateTime Fecha_Emision { get; set; }

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(Pedido))]
        public int Pedido_Id { get; set; }
        public Pedido? Pedido { get; set; }

        public List<DetalleFactura> DetallesFactura { get; set; } = new();
    }
}