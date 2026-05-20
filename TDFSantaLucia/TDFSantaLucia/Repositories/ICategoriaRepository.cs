using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface ICategoriaRepository
    {
        List<Categoria> ObtenerTodos();
        Categoria? ObtenerPorId(int id);
        bool ExisteNombre(string nombre);
        bool ExisteNombreEnOtra(string nombre, int idExcluir);
        void Agregar(Categoria categoria);
        void Actualizar(Categoria categoria);
        void Eliminar(int id);
    }
}