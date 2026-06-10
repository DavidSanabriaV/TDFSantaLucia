using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface ICarritoService
    {
        List<CarritoItem> ObtenerCarrito();

        void AgregarItem(CarritoItem item);
        void ActualizarCantidad(int productoId, int cantidad);
        void EliminarItem(int productoId);
        void LimpiarCarrito();

        Task AgregarItemAsync(CarritoItem item);
        Task ActualizarCantidadAsync(int productoId, int cantidad);
        Task EliminarItemAsync(int productoId);
        Task LimpiarCarritoAsync();

        int ContarItems();
        decimal ObtenerTotal();
        Task SincronizarSesionADbAsync();
        Task SincronizarDbASesionAsync();
    }
}