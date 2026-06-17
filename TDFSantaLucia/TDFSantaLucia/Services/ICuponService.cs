using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface ICuponService
    {
        List<Cupon> ObtenerTodos();
        Cupon? ObtenerPorId(int id);
        List<ClienteCupon> ObtenerCuponesCliente(int clienteId);
        (bool exito, string? error) CrearCupon(CuponViewModel model, string usuarioId);
        (bool exito, string? error) ActualizarCupon(CuponViewModel model);
        (bool exito, string? error) EliminarCupon(int id);
        (bool exito, string? error) AsignarCuponACliente(int cuponId, int clienteId);
        (bool exito, string? error) AsignarCuponATodos(int cuponId, string? filtroRol);
        (bool exito, decimal descuento, int clienteCuponId) ValidarYCalcularDescuento(int cuponId, int clienteId, decimal total);
        void MarcarComoUtilizado(int clienteCuponId);
        (bool exito, string? error) DevolverCupon(int clienteCuponId);
    }
}