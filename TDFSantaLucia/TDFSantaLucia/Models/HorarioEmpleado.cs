using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TDFSantaLucia.Models
{
    public class HorarioEmpleado
    {
        [Key]
        public int Horario_Empleado_Id { get; set; }

        public DateTime Fecha { get; set; }

        public TimeSpan Hora_Ingreso { get; set; }

        public TimeSpan Hora_Salida { get; set; }

        public TimeSpan Inicio_Almuerzo { get; set; }

        public TimeSpan Fin_Almuerzo { get; set; }

        [ForeignKey(nameof(Empleado))]
        public int Empleado_Id { get; set; }
        public Empleado? Empleado { get; set; }
    }
}