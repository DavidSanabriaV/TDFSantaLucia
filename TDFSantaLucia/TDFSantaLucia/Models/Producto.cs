using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Producto
    {
        [Key]
        public int Producto_Id { get; set; }

        [ForeignKey(nameof(Categoria))]
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        public int Categoria_Id { get; set; }
        public Categoria? Categoria { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(150, ErrorMessage = "El nombre no puede superar 150 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        
        public string? Descripcion { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        [Range(500, 9999999, ErrorMessage = "El precio debe ser al menos ₡500.")]
        public decimal Precio { get; set; }

        [StringLength(100)]
        public string? Marca { get; set; }

        public bool Estado { get; set; }

        [StringLength(500)]
        public string? Imagen_URL { get; set; }

        public bool Receta { get; set; }

        public List<Inventario> Inventarios { get; set; } = new();
        public List<DetallePedido> DetallesPedido { get; set; } = new();
        public List<DetalleFactura> DetallesFactura { get; set; } = new();
    }
}