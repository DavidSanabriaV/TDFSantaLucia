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
        (bool exito, string? error) Desactivar(int id);
        (bool exito, string? error) Activar(int id);
        bool ExisteAsync(int id);
        bool ExisteNombre(string nombre);
        bool ExisteNombreEnOtra(string nombre, int id);
    }
}