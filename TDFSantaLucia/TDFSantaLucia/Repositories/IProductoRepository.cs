using TDFSantaLucia.Models;
using System.Collections.Generic;

namespace TDFSantaLucia.Repositories
{
    public interface IProductoRepository
    {
        List<Producto> ObtenerTodos();
        Producto? ObtenerPorId(int id);
        void Agregar(Producto entidad);
        void Actualizar(Producto entidad);
        void Eliminar(int id);
        bool ExisteNombre(string nombre);
        bool ExisteNombreEnOtra(string nombre, int id);
    }
}