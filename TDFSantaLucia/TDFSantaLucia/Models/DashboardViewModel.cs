namespace TDFSantaLucia.Models
{
    public class DashboardViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // KPIs de ventas
        public decimal TotalIngresoVentas { get; set; }
        public int TotalFacturas { get; set; }
        public long TotalUnidadesVendidas { get; set; }
        public decimal TicketPromedio =>
            TotalFacturas > 0 ? Math.Round(TotalIngresoVentas / TotalFacturas, 2) : 0;

        // Inventario
        public List<InventarioAlertaVM> ProductosStockBajo { get; set; } = new();
        public int TotalStockBajo { get; set; }
        public List<InventarioAlertaVM> ProductosProximosVencer { get; set; } = new();
        public int TotalProximosVencer { get; set; }
        public int DiasAlertaVencimientoProducto { get; set; } = 30;

        // Ventas
        public List<ProductoTopVentaVM> TopProductos { get; set; } = new();
        public ProductoTopVentaVM? ProductoMasVendido => TopProductos.FirstOrDefault();
        public List<CategoriaVentaVM> VentasPorCategoria { get; set; } = new();
        public List<ResumenMensualVM> ResumenMensual { get; set; } = new();
        public List<VentaDiariaVM> VentasDiarias { get; set; } = new();

        // Recetas médicas
        public List<RecetaPorVencerVM> RecetasPorVencer { get; set; } = new();
        public int TotalRecetasPorVencer { get; set; }
        public int DiasAlertaReceta { get; set; } = 15;

        // Citas
        public List<CitaResumenVM> CitasHoy { get; set; } = new();
        public int TotalCitasHoy { get; set; }
        public int TotalCitasPendientes { get; set; }

        // Tratamientos
        public int TotalTratamientosActivos { get; set; }

        // Cupones
        public int TotalCuponesActivos { get; set; }
        public int TotalCuponesProximosVencer { get; set; }
        public int TotalCuponesUtilizadosPeriodo { get; set; }
        public int DiasAlertaCupon { get; set; } = 15;

        // Puntos
        public int TotalPuntosOtorgadosPeriodo { get; set; }
        public int TotalPuntosCanjeadosPeriodo { get; set; }
    }
}