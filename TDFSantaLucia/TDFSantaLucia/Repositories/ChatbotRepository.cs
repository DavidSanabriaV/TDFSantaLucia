using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using Microsoft.EntityFrameworkCore;

namespace TDFSantaLucia.Repositories
{
    public class ChatbotRepository : IChatbotRepository
    {
        private readonly AppDbContext _db;

        public ChatbotRepository(AppDbContext db) => _db = db;

        public List<ChatbotOpcion> ObtenerTodas()
            => _db.ChatbotOpciones
                .OrderBy(o => o.Orden)
                .ToList();

        public List<ChatbotOpcion> ObtenerActivas()
            => _db.ChatbotOpciones
                .Where(o => o.Activo)
                .OrderBy(o => o.Orden)
                .ToList();

        public ChatbotOpcion? ObtenerPorId(int id)
            => _db.ChatbotOpciones.Find(id);

        public void Agregar(ChatbotOpcion opcion)
        {
            _db.ChatbotOpciones.Add(opcion);
            _db.SaveChanges();
        }

        public void Actualizar(ChatbotOpcion opcion)
        {
            var existente = _db.ChatbotOpciones.Local
                .FirstOrDefault(o => o.Opcion_Id == opcion.Opcion_Id);
            if (existente != null)
                _db.Entry(existente).State = EntityState.Detached;

            _db.ChatbotOpciones.Update(opcion);
            _db.SaveChanges();
        }

        public void Eliminar(int id)
        {
            var opcion = _db.ChatbotOpciones.Find(id);
            if (opcion != null)
            {
                _db.ChatbotOpciones.Remove(opcion);
                _db.SaveChanges();
            }
        }
    }
}