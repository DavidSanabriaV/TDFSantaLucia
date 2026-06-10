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
        int ContarItems();
        decimal ObtenerTotal();
    }
}