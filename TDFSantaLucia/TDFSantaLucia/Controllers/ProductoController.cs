using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Authorize]
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

        [AllowAnonymous]
        public IActionResult Index(string? buscar)
        {
            ViewData["Title"] = "Productos";
            var productos = _productoService.ObtenerTodos();
            ViewBag.BuscarInicial = buscar ?? "";
            return View(productos);
        }

        [HttpGet("sugerencias")]
        public IActionResult Sugerencias()
        {
            var productos = _productoService.ObtenerTodos()
                .Where(p => p.Estado)
                .Select(p => new
                {
                    id = p.Producto_Id,
                    nombre = p.Nombre,
                    precio = p.Precio,
                    imagen = p.Imagen_URL
                });

            return Json(productos);
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
            if (_productoService.ExisteNombre(producto.Nombre?.Trim() ?? ""))
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre.");

            if (ModelState.IsValid)
            {
                producto.Estado = false;
                _productoService.Crear(producto);
                TempData["ExitoProducto"] = "Producto creado. Estará inactivo hasta que se registre stock en inventario.";
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
            if (id != producto.Producto_Id)
                return NotFound();

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
                    var (exito, error) = _productoService.Actualizar(producto);

                    if (!exito)
                    {
                        ModelState.AddModelError("Estado", error!);
                        CargarCategorias(producto.Categoria_Id);
                        ViewData["Title"] = "Editar Producto";
                        return View(producto);
                    }

                    TempData["ExitoProducto"] = "Producto actualizado exitosamente.";
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

        [HttpPost("eliminar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarConfirmado(int id)
        {
            var producto = _productoService.ObtenerPorId(id);

            if (producto == null)
                return NotFound();

            var resultado = _productoService.Eliminar(id);

            if (!resultado.exito)
            {
                TempData["ErrorProducto"] = resultado.error;
                return RedirectToAction(nameof(Index));
            }

            TempData["ExitoProducto"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
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