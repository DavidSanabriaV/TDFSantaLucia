using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _env;

        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };
        private const long TamanoMaximoBytes = 5 * 1024 * 1024; // 5 MB

        public ProductoController(
            IProductoService productoService,
            ICategoriaService categoriaService,
            ICarritoService carritoService,
            IWebHostEnvironment env)
        {
            _productoService = productoService;
            _categoriaService = categoriaService;
            _carritoService = carritoService;
            _env = env;
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

        [Authorize(Roles = "Admin,Empleado")]
        [HttpGet("crear")]
        public IActionResult Crear()
        {
            CargarCategorias();
            ViewData["Title"] = "Nuevo Producto";
            return View(new Producto());
        }

        [Authorize(Roles = "Admin,Empleado")]
        [HttpPost("crear")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            [Bind("Categoria_Id,Nombre,Descripcion,Precio,Marca,Estado,Receta")]
            Producto producto,
            IFormFile? ImagenArchivo)
        {
            if (_productoService.ExisteNombre(producto.Nombre?.Trim() ?? ""))
                ModelState.AddModelError("Nombre", "Ya existe un producto con ese nombre.");

            string? extension = ValidarImagen(ImagenArchivo, requerida: true);

            if (ModelState.IsValid)
            {
                producto.Estado = false;
                producto.Imagen_URL = await GuardarImagenAsync(ImagenArchivo!, extension!);

                _productoService.Crear(producto);

                TempData["ExitoProducto"] =
                    "Producto creado. Estará inactivo hasta que se registre stock en inventario.";

                return RedirectToAction(nameof(Administrar));
            }

            CargarCategorias(producto.Categoria_Id);
            ViewData["Title"] = "Nuevo Producto";
            return View(producto);
        }

        [Authorize(Roles = "Admin,Empleado")]
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

        [Authorize(Roles = "Admin,Empleado")]
        [HttpPost("editar/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            [Bind("Producto_Id,Categoria_Id,Nombre,Descripcion,Precio,Marca,Estado,Receta")]
            Producto producto,
            IFormFile? ImagenArchivo)
        {
            if (id != producto.Producto_Id)
                return NotFound();

            var original = _productoService.ObtenerPorId(id);
            if (original == null)
                return NotFound();

            if (producto.Precio == 0)
            {
                producto.Precio = original.Precio;
                ModelState.Remove("Precio");
            }

            if (_productoService.ExisteNombreEnOtra(producto.Nombre?.Trim() ?? "", id))
                ModelState.AddModelError("Nombre", "Ya existe otro producto con ese nombre.");

            string? extension = ValidarImagen(ImagenArchivo, requerida: false);

            if (ModelState.IsValid)
            {
                try
                {
                    if (ImagenArchivo != null && ImagenArchivo.Length > 0)
                    {
                        producto.Imagen_URL = await GuardarImagenAsync(ImagenArchivo, extension!);
                        EliminarImagenAnterior(original.Imagen_URL);
                    }
                    else
                    {
                        producto.Imagen_URL = original.Imagen_URL;
                    }

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

        [Authorize(Roles = "Admin")]
        [HttpPost("desactivar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Desactivar(int id)
        {
            var (exito, error) = _productoService.Desactivar(id);

            TempData[exito ? "ExitoProducto" : "ErrorProducto"] =
                exito ? "Producto desactivado correctamente." : error;

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("activar/{id:int}")]
        [ValidateAntiForgeryToken]
        public IActionResult Activar(int id)
        {
            var (exito, error) = _productoService.Activar(id);

            TempData[exito ? "ExitoProducto" : "ErrorProducto"] =
                exito ? "Producto activado correctamente." : error;

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("agregaralcarrito")]
        public IActionResult AgregarAlCarrito(int productoId, int cantidad = 1)
        {
            var producto = _productoService.ObtenerPorId(productoId);

            if (producto == null || !producto.Estado)
            {
                return Json(new { exito = false, mensaje = "Producto no disponible." });
            }

            _carritoService.AgregarItem(new CarritoItem
            {
                Producto_Id = producto.Producto_Id,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Cantidad = cantidad,
                Imagen_URL = producto.Imagen_URL
            });

            return Json(new { exito = true, mensaje = $"{producto.Nombre} agregado al carrito." });
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

        private string? ValidarImagen(IFormFile? archivo, bool requerida)
        {
            if (archivo == null || archivo.Length == 0)
            {
                if (requerida)
                    ModelState.AddModelError("Imagen_URL", "Debe seleccionar una imagen.");
                return null;
            }

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError("Imagen_URL", "Formato no permitido. Use JPG, PNG o WEBP.");
                return null;
            }

            if (archivo.Length > TamanoMaximoBytes)
            {
                ModelState.AddModelError("Imagen_URL", "La imagen no puede superar 5 MB.");
                return null;
            }

            return extension;
        }

        private async Task<string> GuardarImagenAsync(IFormFile archivo, string extension)
        {
            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var carpeta = Path.Combine(_env.WebRootPath, "img", "productos");

            Directory.CreateDirectory(carpeta);

            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/img/productos/{nombreArchivo}";
        }

        private void EliminarImagenAnterior(string? rutaAnterior)
        {
            if (string.IsNullOrWhiteSpace(rutaAnterior)) return;
            if (!rutaAnterior.StartsWith("/img/productos/")) return;

            var rutaFisica = Path.Combine(_env.WebRootPath, rutaAnterior.TrimStart('/'));

            if (System.IO.File.Exists(rutaFisica))
                System.IO.File.Delete(rutaFisica);
        }
    }
}