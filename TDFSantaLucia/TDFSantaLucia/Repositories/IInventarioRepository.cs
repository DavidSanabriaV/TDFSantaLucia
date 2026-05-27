using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IInventarioRepository
    {
        List<Inventario> ObtenerTodos();
        Inventario? ObtenerPorId(int id);
        List<Inventario> ObtenerPorProducto(int productoId);
        List<Inventario> ObtenerStockBajo();
        List<Inventario> ObtenerProximosAVencer(int diasAlerta);
        bool ExisteNumeroLote(string numeroLote);
        bool ExisteNumeroLoteEnOtro(string numeroLote, int idExcluir);
        void Agregar(Inventario inventario);
        void Actualizar(Inventario inventario);
        void Eliminar(int id);
    }
}