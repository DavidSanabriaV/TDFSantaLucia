using System.ComponentModel.DataAnnotations;

namespace TDFSantaLucia.Models
{
    public class ChatbotOpcion
    {
        [Key]
        public int Opcion_Id { get; set; }

        [Required]
        public string Texto { get; set; } = string.Empty;
        public string? Respuesta { get; set; }

        public string? Icono { get; set; }


        public string? Intent { get; set; }

        public string? Url_Redireccion { get; set; }


        public int Orden { get; set; } = 0;

        public bool Activo { get; set; } = true;

        public DateTime Fecha_Creacion { get; set; } = DateTime.Now;
        public DateTime Fecha_Actualizacion { get; set; } = DateTime.Now;
    }

    public static class ChatbotIntents
    {
        public const string VerPedidos = "ver_pedidos";
        public const string VerCitas = "ver_citas";
        public const string VerFacturas = "ver_facturas";
        public const string VerCarrito = "ver_carrito";
        public const string Horario = "horario";
        public const string Ubicacion = "ubicacion";
        public const string Contacto = "contacto";
    }
}