using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("producto")]
    public class ProductoController : Controller
    {
        private readonly IProductoService _productoService;
        private readonly ICategoriaService _categoriaService;
        private readonly ICarritoService _carritoService;

        public ProductoController(
            IProductoService productoService,
            ICategoriaService categoriaService,
            ICarritoService carritoService)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _carritoService = carritoService;
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewData["Title"] = "Productos";
            var productos = _productoService.ObtenerTodos()
                .Where(p => p.Estado)
                .ToList();
            return View(productos);
        }

        [HttpGet("administrar")]
        public IActionResult Administrar()
            => RedirectToAction(nameof(Index));

        [HttpGet("detalle/{id:int}")]
        public IActionResult Detalle(int? id)
        {
            if (id == null) return NotFound();
            var producto = _productoService.ObtenerPorId(id.Value);
            if (producto == null) return NotFound();
            ViewData["Title"] = "Detalle de Producto";
            return View(producto);
        }

        [HttpGet("crear")]
        public IActionResult Crear()
        {
            CargarCategorias();
            ViewData["Title"] = "Nuevo Producto";
            return View(new Producto());
        }

        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(
            [Bind("Categoria_Id,Nombre,Descripcion,Precio,Marca,Estado,Imagen_URL,Receta")]
            Producto producto)
        {
            // Evitar nombre duplicado
            if (_productoService.ExisteNombre(producto.Nombre?.Trim() ?? ""))
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre.");

            if (ModelState.IsValid)
            {
                _productoService.Crear(producto);
                TempData["Exito"] = "Producto creado exitosamente.";
                // PRG: evita doble submit al recargar
                return RedirectToAction(nameof(Administrar));
            }

            CargarCategorias(producto.Categoria_Id);
            ViewData["Title"] = "Nuevo Producto";
            return View(producto);
        }

        [HttpGet("editar/{id:int}")]
        public IActionResult Editar(int? id)
        {
            if (id == null) return NotFound();
            var producto = _productoService.ObtenerPorId(id.Value);
            if (producto == null) return NotFound();
            CargarCategorias(producto.Categoria_Id);
            ViewData["Title"] = "Editar Producto";
            return View(producto);
        }

        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(
    int id,
    [Bind("Producto_Id,Categoria_Id,Nombre,Descripcion,Precio,Marca,Estado,Imagen_URL,Receta")]
    Producto producto)
        {
            if (id != producto.Producto_Id) return NotFound();

            if (producto.Precio == 0)
            {
                var original = _productoService.ObtenerPorId(id);
                if (original != null)
                {
                    producto.Precio = original.Precio;
                    ModelState.Remove("Precio");
                }
            }

            if (_productoService.ExisteNombreEnOtra(producto.Nombre?.Trim() ?? "", id))
                ModelState.AddModelError("Nombre", "Ya existe otro producto con ese nombre.");

            if (ModelState.IsValid)
            {
                try
                {
                    _productoService.Actualizar(producto);
                    TempData["Exito"] = "Producto actualizado exitosamente.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_productoService.ExisteAsync(producto.Producto_Id))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Administrar));
            }

            CargarCategorias(producto.Categoria_Id);
            ViewData["Title"] = "Editar Producto";
            return View(producto);
        }

        [HttpGet("eliminar/{id:int}")]
        public IActionResult Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var producto = _productoService.ObtenerPorId(id.Value);
            if (producto == null) return NotFound();
            ViewData["Title"] = "Eliminar Producto";
            return View(producto);
        }

        [HttpPost("eliminar/{id:int}")]
        [ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var producto = _productoService.ObtenerPorId(id);
            if (producto == null) return NotFound();

            // Eliminación REAL en base de datos
            _productoService.Eliminar(id);

            TempData["Exito"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Administrar));
        }

        [HttpPost("agregaralcarrito")]
        public IActionResult AgregarAlCarrito(int productoId, int cantidad = 1)
        {
            var producto = _productoService.ObtenerPorId(productoId);

            if (producto == null || !producto.Estado)
            {
                return Json(new
                {
                    exito = false,
                    mensaje = "Producto no disponible."
                });
            }

            _carritoService.AgregarItem(new CarritoItem
            {
                Producto_Id = producto.Producto_Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Cantidad = cantidad,
                Imagen_URL = producto.Imagen_URL
            });

            return Json(new
            {
                exito = true,
                mensaje = $"{producto.Nombre} agregado al carrito."
            });
        }

        private void CargarCategorias(int? selectedId = null)
        {
            var categorias = _categoriaService
                .ObtenerTodos()
                .Where(c => c.Estado)
                .OrderBy(c => c.Nombre)
                .ToList();

            ViewBag.Categorias = new SelectList(categorias, "Categoria_Id", "Nombre", selectedId);
        }
    }
}