using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface ICuponRepository
    {
        List<Cupon> ObtenerTodos();
        Cupon? ObtenerPorId(int id);
        List<Cupon> ObtenerDisponibles();
        List<ClienteCupon> ObtenerCuponesCliente(int clienteId);
        ClienteCupon? ObtenerClienteCupon(int clienteId, int cuponId);
        void Agregar(Cupon cupon);
        void Actualizar(Cupon cupon);
        void Eliminar(int id);
        void AsignarCuponACliente(ClienteCupon clienteCupon);
        void MarcarComoUtilizado(int clienteCuponId);
    }
}