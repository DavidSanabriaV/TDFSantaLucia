using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _repo;
        private readonly IInventarioService _inventarioService;

        private const int DiasAlertaProducto = 30;
        private const int DiasAlertaReceta = 15;
        private const int DiasAlertaCupon = 15;

        public DashboardService(IDashboardRepository repo, IInventarioService inventarioService)
        {
            _repo = repo;
            _inventarioService = inventarioService;
        }

        public DashboardViewModel ObtenerDashboard(DateTime fechaInicio, DateTime fechaFin)
        {
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            var (totalIngreso, totalFacturas, totalUnidades) = _repo.ObtenerKpisPeriodo(fechaInicio, fechaFin);

            var topProductos = _repo.ObtenerTopProductos(fechaInicio, fechaFin)
                .Select(x => new ProductoTopVentaVM
                {
                    Nombre = x.nombre,
                    CantidadVendida = x.cantidad,
                    IngresoGenerado = x.ingreso
                }).ToList();

            var ventasPorCategoria = _repo.ObtenerVentasPorCategoria(fechaInicio, fechaFin)
                .Select(x => new CategoriaVentaVM
                {
                    Categoria = x.categoria,
                    CantidadVendida = x.cantidad,
                    IngresoGenerado = x.ingreso
                }).ToList();

            var resumenFacturas = _repo.ObtenerResumenFacturasPorAnio(DateTime.Today.Year)
                .ToDictionary(x => x.mes);
            var unidadesPorMes = _repo.ObtenerUnidadesPorAnio(DateTime.Today.Year)
                .ToDictionary(x => x.mes, x => x.unidades);

            var resumenMensual = Enumerable.Range(1, 12).Select(mes => new ResumenMensualVM
            {
                NumeroMes = mes,
                Mes = meses[mes - 1],
                CantidadFacturas = resumenFacturas.TryGetValue(mes, out var rf) ? rf.cantidadFacturas : 0,
                IngresoVentas = resumenFacturas.TryGetValue(mes, out var rf2) ? rf2.total : 0,
                UnidadesVendidas = unidadesPorMes.TryGetValue(mes, out var u) ? u : 0
            }).ToList();

            var ventasDiarias = _repo.ObtenerVentasDiarias(fechaInicio, fechaFin)
                .Select(x => new VentaDiariaVM
                {
                    Fecha = x.fecha.ToString("dd/MM"),
                    Unidades = x.unidades,
                    Ingreso = x.ingreso
                }).ToList();

            var stockBajo = _inventarioService.ObtenerStockBajo()
                .Select(MapInventarioAlerta).ToList();

            var proximosVencer = _inventarioService.ObtenerProximosAVencer(DiasAlertaProducto)
                .Select(MapInventarioAlerta).ToList();

            var recetasPorVencer = _repo.ObtenerRecetasProximasAVencer(DiasAlertaReceta)
                .Select(r => new RecetaPorVencerVM
                {
                    Receta_Id = r.Receta_Id,
                    Medicamento = r.Producto?.Nombre ?? "—",
                    ClienteNombre = ArmarNombre(r.Expediente?.Cliente?.Usuario),
                    FechaVencimiento = r.Fecha_Vencimiento ?? DateTime.Today
                }).ToList();

            var citasHoy = _repo.ObtenerCitasDelDia(DateTime.Today)
                .Select(c => new CitaResumenVM
                {
                    Cita_Id = c.Cita_Id,
                    Servicio = c.Servicio,
                    Fecha = c.Fecha,
                    Estado = c.Estado,
                    ClienteNombre = ArmarNombre(c.Cliente?.Usuario),
                    EmpleadoNombre = c.Empleado?.Usuario != null
                        ? ArmarNombre(c.Empleado.Usuario)
                        : "Sin asignar"
                }).ToList();

            return new DashboardViewModel
            {
                FechaInicio = fechaInicio,
                FechaFin = fechaFin,

                TotalIngresoVentas = totalIngreso,
                TotalFacturas = totalFacturas,
                TotalUnidadesVendidas = totalUnidades,

                ProductosStockBajo = stockBajo,
                TotalStockBajo = stockBajo.Count,
                ProductosProximosVencer = proximosVencer,
                TotalProximosVencer = proximosVencer.Count,
                DiasAlertaVencimientoProducto = DiasAlertaProducto,

                TopProductos = topProductos,
                VentasPorCategoria = ventasPorCategoria,
                ResumenMensual = resumenMensual,
                VentasDiarias = ventasDiarias,

                RecetasPorVencer = recetasPorVencer,
                TotalRecetasPorVencer = recetasPorVencer.Count,
                DiasAlertaReceta = DiasAlertaReceta,

                CitasHoy = citasHoy,
                TotalCitasHoy = citasHoy.Count,
                TotalCitasPendientes = _repo.ContarCitasPendientes(),

                TotalTratamientosActivos = _repo.ContarTratamientosActivos(),

                TotalCuponesActivos = _repo.ContarCuponesActivos(),
                TotalCuponesProximosVencer = _repo.ContarCuponesProximosAVencer(DiasAlertaCupon),
                TotalCuponesUtilizadosPeriodo = _repo.ContarCuponesUtilizadosPeriodo(fechaInicio, fechaFin),
                DiasAlertaCupon = DiasAlertaCupon,

                TotalPuntosOtorgadosPeriodo = _repo.SumarPuntosPorTipoPeriodo("Ganado", fechaInicio, fechaFin),
                TotalPuntosCanjeadosPeriodo = _repo.SumarPuntosPorTipoPeriodo("Canjeado", fechaInicio, fechaFin)
            };
        }

        private static InventarioAlertaVM MapInventarioAlerta(Inventario i) => new()
        {
            Inventario_Id = i.Inventario_Id,
            Producto_Id = i.Producto_Id,
            ProductoNombre = i.Producto?.Nombre ?? "—",
            Marca = i.Producto?.Marca,
            NumeroLote = i.Numero_Lote,
            Proveedor = i.Proveedor,
            CantidadDisponible = i.Cantidad_Disponible,
            CantidadMinima = i.Cantidad_Minima,
            FechaVencimiento = i.Fecha_Vencimiento
        };

        private static string ArmarNombre(Usuario? usuario)
        {
            if (usuario == null) return "—";
            return $"{usuario.Nombre} {usuario.Primer_Apellido} {usuario.Segundo_Apellido}".Trim();
        }
    }
}