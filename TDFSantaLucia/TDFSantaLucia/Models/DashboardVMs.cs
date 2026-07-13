namespace TDFSantaLucia.Models
{
    public class InventarioAlertaVM
    {
        public int Inventario_Id { get; set; }
        public int Producto_Id { get; set; }
        public string ProductoNombre { get; set; } = string.Empty;
        public string? Marca { get; set; }
        public string? NumeroLote { get; set; }
        public string? Proveedor { get; set; }
        public int CantidadDisponible { get; set; }
        public int CantidadMinima { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public int DiasParaVencer => (FechaVencimiento.Date - DateTime.Today).Days;
    }

    public class ProductoTopVentaVM
    {
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal IngresoGenerado { get; set; }
    }

    public class CategoriaVentaVM
    {
        public string Categoria { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal IngresoGenerado { get; set; }
    }

    public class ResumenMensualVM
    {
        public int NumeroMes { get; set; }
        public string Mes { get; set; } = string.Empty;
        public long UnidadesVendidas { get; set; }
        public decimal IngresoVentas { get; set; }
        public int CantidadFacturas { get; set; }
    }

    public class VentaDiariaVM
    {
        public string Fecha { get; set; } = string.Empty;
        public int Unidades { get; set; }
        public decimal Ingreso { get; set; }
    }

    public class RecetaPorVencerVM
    {
        public int Receta_Id { get; set; }
        public string Medicamento { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public DateTime FechaVencimiento { get; set; }
        public int DiasParaVencer => (FechaVencimiento.Date - DateTime.Today).Days;
    }

    public class CitaResumenVM
    {
        public int Cita_Id { get; set; }
        public string? Servicio { get; set; }
        public DateTime Fecha { get; set; }
        public string? Estado { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string? EmpleadoNombre { get; set; }
    }
}