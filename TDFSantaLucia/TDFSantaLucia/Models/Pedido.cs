using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Pedido
    {
        [Key]
        public int Pedido_Id { get; set; }

        public string? Numero_Orden { get; set; }
        public string? Estado { get; set; }
        public decimal Total { get; set; }
        public string? Descripcion { get; set; }
        public string? Tipo_Entrega { get; set; }
        public string? Metodo_Pago { get; set; }
        public string? Direccion_Entrega { get; set; }
        public string? Telefono_Contacto { get; set; }
        public DateTime Fecha_Creacion { get; set; }
        public DateTime Fecha_Actualizacion { get; set; }

        public string? Receta_URL { get; set; }
        public bool Requiere_Receta { get; set; }
        public string? Estado_Receta { get; set; }

        public int Puntos_Canjeados { get; set; }       
        public decimal Descuento_Puntos { get; set; }   
        public int Puntos_Ganados { get; set; }        
        public bool Uso_Puntos { get; set; }        
        

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