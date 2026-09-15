using TDFSantaLucia.Models;

namespace TDFSantaLucia.Repositories
{
    public interface IEmpleadoRepository
    {
        List<Empleado> ObtenerTodos();
        Empleado? ObtenerPorId(int id);
        void Agregar(Empleado empleado);
        void Actualizar(Empleado empleado);
        void Desactivar(int id);
        void Activar(int id);
    }
}
