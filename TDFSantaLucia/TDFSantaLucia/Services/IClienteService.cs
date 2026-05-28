using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IClienteService
    {
        List<Cliente> ObtenerTodos();
        Cliente? ObtenerPorId(int id);
        Task<(bool exito, string? error)> CrearCliente(ClienteViewModel model);
        Task<(bool exito, string? error)> ActualizarCliente(ClienteViewModel model);
        Task<(bool exito, string? error)> EliminarClienteAsync(int id);
    }
}