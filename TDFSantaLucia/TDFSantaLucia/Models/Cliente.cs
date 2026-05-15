using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class Cliente
    {
        [Key]
        public int Cliente_Id { get; set; }

        [Required]
        public string Cedula { get; set; }

        public string? Telefono { get; set; }

        public string? Direccion_Exacta { get; set; }

        public DateTime? Fecha_Nacimiento { get; set; }

        public int Puntos_Acumulados { get; set; }

        [ForeignKey(nameof(Usuario))]
        public string Usuario_ID { get; set; }
        public Usuario? Usuario { get; set; }

        public List<Pedido> Pedidos { get; set; } = new();
        public List<Factura> Facturas { get; set; } = new();
        public List<Cita> Citas { get; set; } = new();
        public List<Expediente> Expedientes { get; set; } = new();
        public List<Tratamiento> Tratamientos { get; set; } = new();
        public List<ClienteCupon> ClienteCupones { get; set; } = new();
    }
}