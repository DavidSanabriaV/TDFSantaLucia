using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly AppDbContext _db;

        public DashboardRepository(AppDbContext db)
        {
            _db = db;
        }

        public (decimal totalIngreso, int totalFacturas, long totalUnidades) ObtenerKpisPeriodo(DateTime inicio, DateTime fin)
        {
            var facturasQuery = _db.Facturas
                .Where(f => f.Fecha_Emision.Date >= inicio.Date && f.Fecha_Emision.Date <= fin.Date);
            // Si querés excluir anuladas, agregá: && f.Estado != "Anulada"

            var totalIngreso = facturasQuery.Sum(f => (decimal?)f.Total) ?? 0m;
            var totalFacturas = facturasQuery.Count();

            var totalUnidades = _db.DetallesFactura
                .Where(d => d.Factura!.Fecha_Emision.Date >= inicio.Date
                         && d.Factura.Fecha_Emision.Date <= fin.Date)
                .Sum(d => (long?)d.Cantidad) ?? 0L;

            return (totalIngreso, totalFacturas, totalUnidades);
        }

        public List<(string nombre, int cantidad, decimal ingreso)> ObtenerTopProductos(DateTime inicio, DateTime fin, int top = 5)
            => _db.DetallesFactura
                .Where(d => d.Factura!.Fecha_Emision.Date >= inicio.Date
                         && d.Factura.Fecha_Emision.Date <= fin.Date)
                .GroupBy(d => d.Producto!.Nombre)
                .Select(g => new
                {
                    Nombre = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Ingreso = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.Cantidad)
                .Take(top)
                .ToList()
                .Select(x => (x.Nombre, x.Cantidad, x.Ingreso))
                .ToList();

        public List<(string categoria, int cantidad, decimal ingreso)> ObtenerVentasPorCategoria(DateTime inicio, DateTime fin)
            => _db.DetallesFactura
                .Where(d => d.Factura!.Fecha_Emision.Date >= inicio.Date
                         && d.Factura.Fecha_Emision.Date <= fin.Date
                         && d.Producto!.Categoria != null)
                .GroupBy(d => d.Producto!.Categoria!.Nombre)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Cantidad = g.Sum(d => d.Cantidad),
                    Ingreso = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(x => x.Ingreso)
                .ToList()
                .Select(x => (x.Categoria, x.Cantidad, x.Ingreso))
                .ToList();

        public List<(int mes, int cantidadFacturas, decimal total)> ObtenerResumenFacturasPorAnio(int anio)
            => _db.Facturas
                .Where(f => f.Fecha_Emision.Year == anio)
                .GroupBy(f => f.Fecha_Emision.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Cantidad = g.Count(),
                    Total = g.Sum(f => f.Total)
                })
                .ToList()
                .Select(x => (x.Mes, x.Cantidad, x.Total))
                .ToList();

        public List<(int mes, long unidades)> ObtenerUnidadesPorAnio(int anio)
            => _db.DetallesFactura
                .Where(d => d.Factura!.Fecha_Emision.Year == anio)
                .GroupBy(d => d.Factura!.Fecha_Emision.Month)
                .Select(g => new
                {
                    Mes = g.Key,
                    Unidades = g.Sum(d => (long)d.Cantidad)
                })
                .ToList()
                .Select(x => (x.Mes, x.Unidades))
                .ToList();

        public List<(DateTime fecha, int unidades, decimal ingreso)> ObtenerVentasDiarias(DateTime inicio, DateTime fin)
            => _db.DetallesFactura
                .Where(d => d.Factura!.Fecha_Emision.Date >= inicio.Date
                         && d.Factura.Fecha_Emision.Date <= fin.Date)
                .GroupBy(d => d.Factura!.Fecha_Emision.Date)
                .Select(g => new
                {
                    Fecha = g.Key,
                    Unidades = g.Sum(d => d.Cantidad),
                    Ingreso = g.Sum(d => d.Subtotal)
                })
                .OrderBy(x => x.Fecha)
                .ToList()
                .Select(x => (x.Fecha, x.Unidades, x.Ingreso))
                .ToList();

        public List<RecetaMedica> ObtenerRecetasProximasAVencer(int diasAlerta)
        {
            var hoy = DateTime.Today;
            var limite = hoy.AddDays(diasAlerta);

            return _db.RecetasMedicas
                .Include(r => r.Producto)
                .Include(r => r.Expediente)
                    .ThenInclude(e => e!.Cliente)
                        .ThenInclude(c => c!.Usuario)
                .Where(r => r.Fecha_Vencimiento.HasValue
                         && r.Fecha_Vencimiento.Value.Date >= hoy
                         && r.Fecha_Vencimiento.Value.Date <= limite)
                .OrderBy(r => r.Fecha_Vencimiento)
                .ToList();
        }

        public List<Cita> ObtenerCitasDelDia(DateTime fecha)
            => _db.Citas
                .Include(c => c.Cliente).ThenInclude(cl => cl!.Usuario)
                .Include(c => c.Empleado).ThenInclude(e => e!.Usuario)
                .Where(c => c.Fecha.Date == fecha.Date)
                .OrderBy(c => c.Fecha)
                .ToList();

        public int ContarCitasPendientes()
             => _db.Citas.Count(c => c.Estado == "En Proceso" && c.Fecha >= DateTime.Now);

        public int ContarTratamientosActivos()
            => _db.Tratamientos.Count(t => t.Estado && t.Fecha_Fin.Date >= DateTime.Today);

        public int ContarCuponesActivos()
            => _db.Cupones.Count(c => c.Estado && c.Fecha_Expiracion.Date >= DateTime.Today);

        public int ContarCuponesProximosAVencer(int diasAlerta)
        {
            var hoy = DateTime.Today;
            var limite = hoy.AddDays(diasAlerta);
            return _db.Cupones.Count(c => c.Estado
                && c.Fecha_Expiracion.Date >= hoy
                && c.Fecha_Expiracion.Date <= limite);
        }

        public int ContarCuponesUtilizadosPeriodo(DateTime inicio, DateTime fin)
            => _db.ClientesCupones.Count(cc => cc.Utilizado
                && cc.Fecha_Uso.HasValue
                && cc.Fecha_Uso.Value.Date >= inicio.Date
                && cc.Fecha_Uso.Value.Date <= fin.Date);

        public int SumarPuntosPorTipoPeriodo(string tipo, DateTime inicio, DateTime fin)
        {
            var suma = _db.MovimientosPuntos
                .Where(m => m.Tipo == tipo
                         && m.Fecha.Date >= inicio.Date
                         && m.Fecha.Date <= fin.Date)
                .Sum(m => (int?)m.Puntos) ?? 0;

            return Math.Abs(suma); // "Canjeado" se guarda negativo
        }
    }
}