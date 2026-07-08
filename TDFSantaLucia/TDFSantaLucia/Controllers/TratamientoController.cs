using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Authorize]
    [Route("tratamiento")]
    public class TratamientoController : Controller
    {
        private readonly ITratamientoService _tratamientoService;
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _db;

        public TratamientoController(
            ITratamientoService tratamientoService,
            UserManager<Usuario> userManager,
            AppDbContext db)
        {
            _tratamientoService = tratamientoService;
            _userManager = userManager;
            _db = db;
        }

        private async Task<Cliente?> ObtenerClienteAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return null;
            return await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuario.Id);
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var tratamientos = _tratamientoService
                .ObtenerPorCliente(cliente.Cliente_Id);

            return View(tratamientos);
        }

        [HttpGet("crear")]
        public IActionResult Crear()
        {
            return View(new TratamientoViewModel
            {
                Fecha_Inicio = DateTime.Today,
                Fecha_Fin = DateTime.Today.AddDays(7),
                Alertas_Activas = true
            });
        }

        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(TratamientoViewModel model,
            [FromForm] List<string> horarios)
        {
            model.Horarios = horarios
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToList();

            if (!ModelState.IsValid)
                return View(model);

            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var (exito, error) = _tratamientoService
                .Crear(model, cliente.Cliente_Id);

            if (!exito)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            TempData["Exito"] = "Tratamiento registrado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet("editar/{id:int}")]
        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var tratamiento = _tratamientoService.ObtenerPorId(id);
            if (tratamiento == null || tratamiento.Cliente_Id != cliente.Cliente_Id)
                return NotFound();

            var model = new TratamientoViewModel
            {
                Tratamiento_Id = tratamiento.Tratamiento_Id,
                Nombre_Medicamento = tratamiento.Nombre_Medicamento ?? "",
                Dosis = tratamiento.Dosis,
                Duracion = tratamiento.Duracion,
                Fecha_Inicio = tratamiento.Fecha_Inicio,
                Fecha_Fin = tratamiento.Fecha_Fin,
                Estado = tratamiento.Estado,
                Alertas_Activas = tratamiento.Alertas_Activas,
                Horarios = tratamiento.Recordatorios
                    .Select(r => r.Hora.ToString(@"hh\:mm"))
                    .ToList()
            };

            return View(model);
        }

        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id,
            TratamientoViewModel model,
            [FromForm] List<string> horarios)
        {
            model.Horarios = horarios
                .Where(h => !string.IsNullOrWhiteSpace(h))
                .ToList();

            if (!ModelState.IsValid)
                return View(model);

            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var (exito, error) = _tratamientoService.Actualizar(id, model);
            if (!exito)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            TempData["Exito"] = "Tratamiento actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost("eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var (exito, error) = _tratamientoService
                .Eliminar(id, cliente.Cliente_Id);

            if (!exito)
                TempData["Error"] = error;
            else
                TempData["Exito"] = "Tratamiento eliminado correctamente.";

            return RedirectToAction("Index");
        }

        [HttpPost("toggle-alertas/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAlertas(int id)
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return Json(new { exito = false });

            var (exito, error) = _tratamientoService
                .ToggleAlertas(id, cliente.Cliente_Id);

            return Json(new { exito, error });
        }

        [HttpGet("recordatorios")]
        public async Task<IActionResult> ObtenerRecordatorios()
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return Json(new List<object>());

            var ahora = TimeSpan.FromTicks(DateTime.Now.TimeOfDay.Ticks);
            var margen = TimeSpan.FromMinutes(5);

            var recordatorios = _tratamientoService
                .ObtenerRecordatoriosActivos(cliente.Cliente_Id)
                .Where(r => r.Hora >= ahora - margen
                         && r.Hora <= ahora + margen)
                .Select(r => new
                {
                    r.Recordatorio_Id,
                    Medicamento = r.Tratamiento?.Nombre_Medicamento,
                    Dosis = r.Tratamiento?.Dosis,
                    Hora = r.Hora.ToString(@"hh\:mm")
                })
                .ToList();

            return Json(recordatorios);
        }
    }
}