using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IDashboardRepository
    {
        (decimal totalIngreso, int totalFacturas, long totalUnidades) ObtenerKpisPeriodo(DateTime inicio, DateTime fin);
        List<(string nombre, int cantidad, decimal ingreso)> ObtenerTopProductos(DateTime inicio, DateTime fin, int top = 5);
        List<(string categoria, int cantidad, decimal ingreso)> ObtenerVentasPorCategoria(DateTime inicio, DateTime fin);
        List<(int mes, int cantidadFacturas, decimal total)> ObtenerResumenFacturasPorAnio(int anio);
        List<(int mes, long unidades)> ObtenerUnidadesPorAnio(int anio);
        List<(DateTime fecha, int unidades, decimal ingreso)> ObtenerVentasDiarias(DateTime inicio, DateTime fin);

        List<RecetaMedica> ObtenerRecetasProximasAVencer(int diasAlerta);

        List<Cita> ObtenerCitasDelDia(DateTime fecha);
        int ContarCitasPendientes();

        int ContarTratamientosActivos();

        int ContarCuponesActivos();
        int ContarCuponesProximosAVencer(int diasAlerta);
        int ContarCuponesUtilizadosPeriodo(DateTime inicio, DateTime fin);

        int SumarPuntosPorTipoPeriodo(string tipo, DateTime inicio, DateTime fin);
    }
}