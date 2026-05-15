using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace TDFSantaLucia.Models
{
    public class ClienteCupon
    {
        [Key]
        public int Cupon_Cliente_Id { get; set; }

        public DateTime Fecha_Asignacion { get; set; }

        public bool Utilizado { get; set; }

        public DateTime? Fecha_Uso { get; set; }

        [ForeignKey(nameof(Cliente))]
        public int Cliente_Id { get; set; }
        public Cliente? Cliente { get; set; }

        [ForeignKey(nameof(Cupon))]
        public int Cupon_Id { get; set; }
        public Cupon? Cupon { get; set; }
    }
}