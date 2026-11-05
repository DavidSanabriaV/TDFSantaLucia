using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface ICarritoService
    {
        List<CarritoItem> ObtenerCarrito();

        (bool Exito, string Mensaje) AgregarItem(CarritoItem item);
        (bool Exito, string Mensaje, int StockMax) ActualizarCantidad(int productoId, int cantidad);
        void EliminarItem(int productoId);
        void LimpiarCarrito();

        Task<(bool Exito, string Mensaje)> AgregarItemAsync(CarritoItem item);
        Task<(bool Exito, string Mensaje, int StockMax)> ActualizarCantidadAsync(int productoId, int cantidad);
        Task EliminarItemAsync(int productoId);
        Task LimpiarCarritoAsync();

        int ObtenerStockDisponible(int productoId);
        int ContarItems();
        decimal ObtenerTotal();
        Task SincronizarSesionADbAsync();
        Task SincronizarDbASesionAsync();
    }
}