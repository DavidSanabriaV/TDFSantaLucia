using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface ICategoriaService
    {
        List<Categoria> ObtenerTodos();
        Categoria? ObtenerDetalle(int id);
        (bool exito, string? error) CrearCategoria(Categoria categoria);
        (bool exito, string? error) ActualizarCategoria(int id, Categoria categoria);
        (bool exito, string? error) EliminarCategoria(int id);
    }
}