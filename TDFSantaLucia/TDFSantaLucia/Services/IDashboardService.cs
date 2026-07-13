using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IDashboardService
    {
        DashboardViewModel ObtenerDashboard(DateTime fechaInicio, DateTime fechaFin);
    }
}