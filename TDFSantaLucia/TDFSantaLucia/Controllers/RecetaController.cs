using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Authorize]
    [Route("receta")]
    public class RecetaController : Controller
    {
        private readonly IRecetaService _recetaService;
        private readonly IExpedienteService _expedienteService;
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _db;

        public RecetaController(
            IRecetaService recetaService,
            IExpedienteService expedienteService,
            UserManager<Usuario> userManager,
            AppDbContext db)
        {
            _recetaService = recetaService;
            _expedienteService = expedienteService;
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

        [HttpGet("crear/{expedienteId:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Crear(int expedienteId)
        {
            var expediente = _expedienteService.ObtenerDetalle(expedienteId);
            if (expediente == null) return NotFound();

            CargarProductos();
            ViewBag.Expediente = expediente;

            return View(new RecetaMedica
            {
                Expediente_Id = expedienteId,
                Fecha_Emision = DateTime.Now
            });
        }

        [HttpPost("crear/{expedienteId:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(int expedienteId, RecetaMedica receta)
        {
            receta.Expediente_Id = expedienteId;

            if (!ModelState.IsValid)
            {
                CargarProductos();
                ViewBag.Expediente =
                    _expedienteService.ObtenerDetalle(expedienteId);
                return View(receta);
            }

            var (exito, error) = _recetaService.Crear(receta);
            if (!exito)
            {
                ModelState.AddModelError("", error!);
                CargarProductos();
                ViewBag.Expediente =
                    _expedienteService.ObtenerDetalle(expedienteId);
                return View(receta);
            }

            TempData["ExitoExpediente"] = "Receta médica registrada correctamente.";
            return RedirectToAction("Detalle", "Expediente",
                new { id = expedienteId });
        }

        [HttpGet("editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Editar(int id)
        {
            var receta = _recetaService.ObtenerPorId(id);
            if (receta == null) return NotFound();

            CargarProductos(receta.Producto_Id);
            ViewBag.Expediente =
                _expedienteService.ObtenerDetalle(receta.Expediente_Id);

            return View(receta);
        }

        [HttpPost("editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, RecetaMedica receta)
        {
            if (!ModelState.IsValid)
            {
                CargarProductos(receta.Producto_Id);
                ViewBag.Expediente =
                    _expedienteService.ObtenerDetalle(receta.Expediente_Id);
                return View(receta);
            }

            var (exito, error) = _recetaService.Actualizar(id, receta);
            if (!exito)
            {
                ModelState.AddModelError("", error!);
                CargarProductos(receta.Producto_Id);
                return View(receta);
            }

            TempData["ExitoExpediente"] = "Receta actualizada correctamente.";
            return RedirectToAction("Detalle", "Expediente",
                new { id = receta.Expediente_Id });
        }

        [HttpPost("eliminar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var receta = _recetaService.ObtenerPorId(id);
            var expedienteId = receta?.Expediente_Id ?? 0;

            var (exito, error) = _recetaService.Eliminar(id);
            if (!exito)
                TempData["ErrorExpediente"] = error;
            else
                TempData["ExitoExpediente"] = "Receta eliminada correctamente.";

            return RedirectToAction("Detalle", "Expediente",
                new { id = expedienteId });
        }

        private void CargarProductos(int? selectedId = null)
        {
            ViewBag.Productos = new SelectList(
                _recetaService.ObtenerProductos()
                    .Select(p => new
                    {
                        p.Producto_Id,
                        Nombre = $"{p.Nombre} {(p.Marca != null ? "- " + p.Marca : "")}"
                    }),
                "Producto_Id", "Nombre", selectedId);
        }

        [HttpGet("descargar/{id:int}")]
        public IActionResult Descargar(int id)
        {
            var receta = _recetaService.ObtenerPorId(id);
            if (receta == null) return NotFound();

            var bytes = _recetaService.GenerarPdf(receta);
            return File(bytes, "text/html",
                $"Receta-{receta.Receta_Id}.html");
        }

        [HttpGet("mis-recetas")]
        [Authorize(Roles = "Cliente")]
        public async Task<IActionResult> MisRecetas()
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null) return RedirectToAction("Index", "Home");

            var recetas = _recetaService.ObtenerPorCliente(cliente.Cliente_Id);
            return View(recetas);
        }
    }
}