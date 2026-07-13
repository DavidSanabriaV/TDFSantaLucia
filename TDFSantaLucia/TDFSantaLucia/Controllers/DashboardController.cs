using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Services;
using System.Text;

namespace TDFSantaLucia.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("dashboard")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("")]
        public IActionResult Index(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var hoy = DateTime.Today;
            var inicio = fechaInicio ?? new DateTime(hoy.Year, hoy.Month, 1);
            var fin = fechaFin ?? hoy;

            if (inicio > fin) (inicio, fin) = (fin, inicio);

            var vm = _dashboardService.ObtenerDashboard(inicio, fin);
            return View(vm);
        }

        [HttpGet("kpis")]
        public IActionResult GetKpis(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var hoy = DateTime.Today;
            var inicio = fechaInicio ?? new DateTime(hoy.Year, hoy.Month, 1);
            var fin = fechaFin ?? hoy;

            var vm = _dashboardService.ObtenerDashboard(inicio, fin);

            return Json(new
            {
                totalIngresoVentas = vm.TotalIngresoVentas.ToString("N0"),
                totalFacturas = vm.TotalFacturas,
                totalUnidadesVendidas = vm.TotalUnidadesVendidas,
                ticketPromedio = vm.TicketPromedio.ToString("N0"),
                stockBajoCount = vm.TotalStockBajo,
                proximosVencerCount = vm.TotalProximosVencer,
                recetasPorVencerCount = vm.TotalRecetasPorVencer,
                citasHoyCount = vm.TotalCitasHoy,
                citasPendientesCount = vm.TotalCitasPendientes,
                tratamientosActivosCount = vm.TotalTratamientosActivos,
                cuponesActivosCount = vm.TotalCuponesActivos,
                puntosOtorgados = vm.TotalPuntosOtorgadosPeriodo,
                puntosCanjeados = vm.TotalPuntosCanjeadosPeriodo,
                productoTopNombre = vm.ProductoMasVendido?.Nombre ?? "Sin datos",
                productoTopCantidad = vm.ProductoMasVendido?.CantidadVendida ?? 0
            });
        }

        [HttpGet("exportar-ventas-csv")]
        public IActionResult ExportarVentasCsv(DateTime? fechaInicio, DateTime? fechaFin)
        {
            var hoy = DateTime.Today;
            var inicio = fechaInicio ?? new DateTime(hoy.Year, hoy.Month, 1);
            var fin = fechaFin ?? hoy;

            var vm = _dashboardService.ObtenerDashboard(inicio, fin);

            var sb = new StringBuilder();
            sb.AppendLine("Producto,Cantidad Vendida,Ingreso Generado");

            foreach (var p in vm.TopProductos)
                sb.AppendLine($"{EscaparCsv(p.Nombre)},{p.CantidadVendida},{p.IngresoGenerado}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(bytes, "text/csv", $"ventas_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.csv");
        }

        // Escapa comillas y neutraliza inyección de fórmulas en Excel (=, +, -, @)
        private static string EscaparCsv(string? valor)
        {
            if (string.IsNullOrEmpty(valor)) return "\"\"";
            var v = valor.Replace("\"", "\"\"");
            if (v.StartsWith("=") || v.StartsWith("+") || v.StartsWith("-") || v.StartsWith("@"))
                v = "'" + v;
            return $"\"{v}\"";
        }
    }
}