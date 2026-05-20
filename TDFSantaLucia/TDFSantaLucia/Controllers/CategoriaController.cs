using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("categoria")]
    public class CategoriaController : Controller
    {
        private readonly ICategoriaService _categoriaService;

        public CategoriaController(ICategoriaService categoriaService)
        {
            _categoriaService = categoriaService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var categorias = _categoriaService.ObtenerTodos();
            return View(categorias);
        }

        [HttpGet("detalle/{id:int}")]
        public IActionResult Detalle(int id)
        {
            var categoria = _categoriaService.ObtenerDetalle(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        [HttpGet("crear")]
        public IActionResult Crear()
        {
            return View(new Categoria { Estado = true });
        }

        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Categoria categoria)
        {
            if (!ModelState.IsValid)
                return View(categoria);

            var (exito, error) = _categoriaService.CrearCategoria(categoria);
            if (!exito)
            {
                ModelState.AddModelError("Nombre", error!);
                return View(categoria);
            }

            TempData["Exito"] = "Categoria creada correctamente";
            return RedirectToAction("Index");
        }

        [HttpGet("editar/{id:int}")]
        public IActionResult Editar(int id)
        {
            var categoria = _categoriaService.ObtenerDetalle(id);
            if (categoria == null)
                return NotFound();

            return View(categoria);
        }

        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Categoria categoria)
        {
            if (id != categoria.Categoria_Id)
                return BadRequest();

            if (!ModelState.IsValid)
                return View(categoria);

            var (exito, error) = _categoriaService.ActualizarCategoria(id, categoria);
            if (!exito)
            {
                ModelState.AddModelError("Nombre", error!);
                return View(categoria);
            }

            TempData["Exito"] = "Categoria actualizada correctamente";
            return RedirectToAction("Index");
        }

        [HttpPost("eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var (exito, error) = _categoriaService.EliminarCategoria(id);
            if (!exito)
                TempData["Error"] = error;
            else
                TempData["Exito"] = "Categoria eliminada correctamente";

            return RedirectToAction("Index");
        }
    }
}