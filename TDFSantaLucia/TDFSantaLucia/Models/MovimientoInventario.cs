using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class MovimientoInventario
    {
        [Key]
        public int Movimiento_Id { get; set; }

        [Required]
        public string Tipo_Movimiento { get; set; } = string.Empty; 

        public int Cantidad { get; set; }

        public string? Descripcion { get; set; } 

        public DateTime Fecha_Movimiento { get; set; } = DateTime.Now;

        [ForeignKey(nameof(Producto))]
        public int Producto_Id { get; set; }
        public Producto? Producto { get; set; }

        [ForeignKey(nameof(Inventario))]
        public int? Inventario_Id { get; set; }
        public Inventario? Inventario { get; set; }

        [ForeignKey(nameof(Pedido))]
        public int? Pedido_Id { get; set; }
        public Pedido? Pedido { get; set; }

        public string? Usuario_Id { get; set; }
        public Usuario? Usuario { get; set; }
    }
}