using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IInventarioService
    {
        List<Inventario> ObtenerTodos();
        Inventario? ObtenerPorId(int id);
        List<Inventario> ObtenerPorProducto(int productoId);
        List<Inventario> ObtenerStockBajo();
        List<Inventario> ObtenerProximosAVencer(int diasAlerta = 30);
        int ContarStockBajo();
        int ContarProximosAVencer(int diasAlerta = 30);
        (bool exito, string? error) Crear(Inventario inventario);
        (bool exito, string? error) Actualizar(int id, Inventario inventario);
        (bool exito, string? error) Eliminar(int id);
        List<Producto> ObtenerProductos();
    }
}