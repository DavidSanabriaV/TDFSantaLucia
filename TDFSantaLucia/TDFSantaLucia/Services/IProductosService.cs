using TDFSantaLucia.Models;
using System.Collections.Generic;

namespace TDFSantaLucia.Services
{
    public interface IProductoService
    {
        List<Producto> ObtenerTodos();
        Producto? ObtenerPorId(int id);
        void Crear(Producto producto);
        void Actualizar(Producto producto);
        void Eliminar(int id);
        bool ExisteAsync(int id);
    }
}