using TDFSantaLucia.Models;
using System.Collections.Generic;

namespace TDFSantaLucia.Services
{
    public interface IProductoService
    {
        List<Producto> ObtenerTodos();
        Producto? ObtenerPorId(int id);
        void Crear(Producto producto);
        (bool exito, string? error) Actualizar(Producto producto);
        void Eliminar(int id);
        bool ExisteAsync(int id);
        bool ExisteNombre(string nombre);
       
        bool ExisteNombreEnOtra(string nombre, int id);
    }
}