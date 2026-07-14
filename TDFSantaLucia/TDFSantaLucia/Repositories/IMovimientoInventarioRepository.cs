using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IMovimientoInventarioRepository
    {
        List<MovimientoInventario> ObtenerPorProducto(int productoId, DateTime? desde, DateTime? hasta, string? tipoFiltro);
        List<MovimientoInventario> ObtenerTodos(DateTime? desde, DateTime? hasta, string? tipoFiltro);
        void Agregar(MovimientoInventario movimiento);
        void GuardarCambios();
    }
}