using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{

    [Route("inventario")]
    public class InventarioController : Controller
    {
        private readonly IInventarioService _service;
        private readonly IMovimientoInventarioRepository _movimientoRepo;
        private const int DiasAlertaVencimiento = 30;

        public InventarioController(
            IInventarioService service,
            IMovimientoInventarioRepository movimientoRepo)
        {
            _service = service;
            _movimientoRepo = movimientoRepo;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            var inventarios = _service.ObtenerTodos();

            ViewBag.TotalStockBajo = _service.ContarStockBajo();
            ViewBag.TotalProximosVencer = _service.ContarProximosAVencer(DiasAlertaVencimiento);

            return View(inventarios);
        }

        [HttpGet("detalle/{id:int}")]
        public IActionResult Detalle(int id)
        {
            var inventario = _service.ObtenerPorId(id);
            if (inventario == null)
                return NotFound();

            return View(inventario);
        }

        [HttpGet("crear")]
        public IActionResult Crear()
        {
            CargarProductos();
            return View(new Inventario
            {
                Fecha_Ingreso = DateTime.Today,
                Estado = true
            });
        }

        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(Inventario inventario)
        {
            if (!ModelState.IsValid)
            {
                CargarProductos();
                return View(inventario);
            }

            var (exito, error) = _service.Crear(inventario);
            if (!exito)
            {
                ModelState.AddModelError("", error!);
                CargarProductos();
                return View(inventario);
            }

            TempData["ExitoInventario"] = "Lote de inventario registrado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpGet("editar/{id:int}")]
        public IActionResult Editar(int id)
        {
            var inventario = _service.ObtenerPorId(id);
            if (inventario == null)
                return NotFound();

            CargarProductos();
            return View(inventario);
        }

        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, Inventario inventario)
        {
            if (id != inventario.Inventario_Id)
                return BadRequest();

            if (!ModelState.IsValid)
            {
                CargarProductos();
                return View(inventario);
            }

            var (exito, error) = _service.Actualizar(id, inventario);
            if (!exito)
            {
                ModelState.AddModelError("", error!);
                CargarProductos();
                return View(inventario);
            }

            TempData["ExitoInventario"] = "Lote actualizado correctamente.";
            return RedirectToAction("Index");
        }

        [HttpPost("eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var (exito, error) = _service.Eliminar(id);
            if (!exito)
                TempData["ErrorInventario"] = error;
            else
                TempData["ExitoInventario"] = "Lote eliminado correctamente.";

            return RedirectToAction("Index");
        }

        [HttpGet("stock-bajo")]
        public IActionResult StockBajo()
        {
            var inventarios = _service.ObtenerStockBajo();
            return View(inventarios);
        }

        [HttpGet("proximos-vencer")]
        public IActionResult ProximosVencer()
        {
            var inventarios = _service.ObtenerProximosAVencer(DiasAlertaVencimiento);
            ViewBag.DiasAlerta = DiasAlertaVencimiento;
            return View(inventarios);
        }

        [HttpGet("por-producto/{productoId:int}")]
        public IActionResult PorProducto(int productoId)
        {
            var inventarios = _service.ObtenerPorProducto(productoId);
            if (!inventarios.Any())
                return NotFound();

            ViewBag.Producto = inventarios.FirstOrDefault()?.Producto?.Nombre ?? "Producto";
            return View(inventarios);
        }

        private void CargarProductos()
        {
            var productos = _service.ObtenerProductos()
                .OrderBy(p => p.Nombre)
                .Select(p => new SelectListItem
                {
                    Value = p.Producto_Id.ToString(),
                    Text = p.Estado
                        ? $"{p.Nombre} {(p.Marca != null ? "- " + p.Marca : "")}"
                        : $"{p.Nombre} {(p.Marca != null ? "- " + p.Marca : "")} ⚠️ Sin stock"
                }).ToList();

            ViewBag.Productos = productos;
        }

        
    }
}