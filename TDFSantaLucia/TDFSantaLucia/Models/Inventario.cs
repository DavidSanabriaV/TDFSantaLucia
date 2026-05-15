using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Inventario
    {
        [Key]
        public int Inventario_Id { get; set; }

        public int Cantidad_Disponible { get; set; }

        public int Cantidad_Minima { get; set; }

        public DateTime Fecha_Vencimiento { get; set; }

        public DateTime Fecha_Ingreso { get; set; }

        public string? Numero_Lote { get; set; }

        public bool Estado { get; set; }

        public string? Proveedor { get; set; }

        [ForeignKey(nameof(Producto))]
        public int Producto_Id { get; set; }
        public Producto? Producto { get; set; }
    }
}