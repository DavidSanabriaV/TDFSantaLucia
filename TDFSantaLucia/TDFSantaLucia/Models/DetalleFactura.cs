using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class DetalleFactura
    {
        [Key]
        public int Detalle_Factura_Id { get; set; }

        public int Cantidad { get; set; }

        public decimal Precio_Unitario { get; set; }

        public decimal Subtotal { get; set; }

        [ForeignKey(nameof(Factura))]
        public int Factura_Id { get; set; }
        public Factura? Factura { get; set; }

        [ForeignKey(nameof(Producto))]
        public int Producto_Id { get; set; }
        public Producto? Producto { get; set; }
    }
}