using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IEmpleadoService
    {
        Task<List<Empleado>> ObtenerTodosAsync();
    Task<Empleado?> ObtenerDetalleAsync(int id);

        Task<EmpleadoViewModel?> ObtenerEmpleadoViewModelAsync(int id);

        Task<(bool success, string? error)> CrearEmpleadoAsync(EmpleadoViewModel model);

        Task<(bool success, string? error)> ActualizarEmpleadoAsync(EmpleadoViewModel model);

        Task<bool> EliminarEmpleadoAsync(int id);

        decimal CalcularSalarioNeto(decimal bruto);

        List<string> ObtenerRoles();
    }


}
