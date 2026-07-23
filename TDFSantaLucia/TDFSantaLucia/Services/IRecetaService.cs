using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public interface IRecetaService
    {
        List<RecetaMedica> ObtenerPorExpediente(int expedienteId);
        RecetaMedica? ObtenerPorId(int id);
        (bool exito, string? error) Crear(RecetaMedica receta);
        (bool exito, string? error) Actualizar(int id, RecetaMedica receta);
        (bool exito, string? error) Eliminar(int id);
        List<Producto> ObtenerProductos();
        byte[] GenerarPdf(RecetaMedica receta);
        List<RecetaMedica> ObtenerPorCliente(int clienteId);
    }
}