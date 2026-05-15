using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Cupon
    {
        [Key]
        public int Cupon_Id { get; set; }

        public string? Descripcion { get; set; }

        public string? Tipo_Descuento { get; set; }

        public decimal Valor_Descuento { get; set; }

        public DateTime Fecha_Expiracion { get; set; }

        public bool Estado { get; set; }

        public DateTime Fecha_Creacion { get; set; }

        [ForeignKey(nameof(Usuario))]
        public string Usuario_Id { get; set; }
        public Usuario? Usuario { get; set; }

        public List<ClienteCupon> ClienteCupones { get; set; } = new();
        public List<Pedido> Pedidos { get; set; } = new();
    }
}