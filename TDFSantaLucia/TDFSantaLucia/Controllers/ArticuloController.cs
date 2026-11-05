using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("salud")]
    public class ArticuloController : Controller
    {
        private const string CarpetaImagenes = "uploads/articulos";
        private const long TamanoMaximo = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] ExtensionesPermitidas =
            { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        private readonly IArticuloService _articuloService;
        private readonly UserManager<Usuario> _userManager;
        private readonly IWebHostEnvironment _env;

        public ArticuloController(
            IArticuloService articuloService,
            UserManager<Usuario> userManager,
            IWebHostEnvironment env)
        {
            _articuloService = articuloService;
            _userManager = userManager;
            _env = env;
        }


        [HttpGet("")]
        public IActionResult Index(string? categoria)
        {
            var articulos = string.IsNullOrEmpty(categoria)
                ? _articuloService.ObtenerTodos(soloPublicados: true)
                : _articuloService.ObtenerPorCategoria(categoria);

            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            ViewBag.Categoria = categoria;
            ViewBag.Categorias = _articuloService
                .ObtenerTodos(soloPublicados: true)
                .Where(a => a.Categoria != null)
                .Select(a => a.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return View(articulos);
        }

        [HttpGet("{id:int}")]
        public IActionResult Detalle(int id)
        {
            var articulo = _articuloService.ObtenerPorId(id);
            if (articulo == null || !articulo.Publicado &&
                !User.IsInRole("Admin") && !User.IsInRole("Empleado"))
                return NotFound();

            var userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;
            ViewBag.UsuarioDioLike = articulo.Likes
                .Any(l => l.Usuario_Id == userId);

            return View(articulo);
        }


        [HttpPost("like/{id:int}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Like(int id)
        {
            var userId = _userManager.GetUserId(User);
            var (exito, totalLikes, usuarioDioLike) =
                await _articuloService.ToggleLikeAsync(id, userId!);

            if (!exito)
                return Json(new { exito = false });

            return Json(new { exito = true, totalLikes, usuarioDioLike });
        }


        [HttpPost("comentar/{id:int}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Comentar(int id, string contenido)
        {
            if (string.IsNullOrWhiteSpace(contenido))
                return Json(new
                {
                    exito = false,
                    error = "El comentario no puede estar vacío."
                });

            var userId = _userManager.GetUserId(User);
            var (exito, error, comentario) =
                await _articuloService.AgregarComentarioAsync(
                    id, contenido.Trim(), userId!);

            if (!exito)
                return Json(new { exito = false, error });

            return Json(new
            {
                exito = true,
                nombre = $"{comentario!.Usuario?.Nombre} " +
                           $"{comentario.Usuario?.Primer_Apellido}",
                contenido = comentario.Contenido,
                fecha = comentario.Fecha_Creacion
                    .ToString("dd/MM/yyyy HH:mm"),
                comentarioId = comentario.Comentario_Id,
                esPropio = true
            });
        }

        [HttpPost("comentario/eliminar/{id:int}")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarComentario(int id)
        {
            var userId = _userManager.GetUserId(User);
            var esAdmin = User.IsInRole("Admin");
            var (exito, error) =
                await _articuloService.EliminarComentarioAsync(
                    id, userId!, esAdmin);

            return Json(new { exito, error });
        }


        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Admin()
        {
            var articulos = _articuloService.ObtenerTodos();
            return View(articulos);
        }

        [HttpGet("crear")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Crear()
            => View(new ArticuloSalud());

        [HttpPost("crear")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ArticuloSalud model)
        {
            ModelState.Remove("Usuario_Id");
            ModelState.Remove("Usuario");
            ModelState.Remove("Usuario.Nombre");
            ModelState.Remove("Usuario.Primer_Apellido");
            ModelState.Remove("Usuario.Segundo_Apellido");
            ModelState.Remove("Fecha_Creacion");
            ModelState.Remove("Fecha_Actualizacion");

            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            model.Usuario = null;
            model.Comentarios = new();
            model.Likes = new();

            if (model.Imagen_Archivo != null && model.Imagen_Archivo.Length > 0)
            {
                var ruta = await GuardarImagenAsync(model.Imagen_Archivo);
                if (ruta == null) return View(model);
                model.Imagen_URL = ruta;
            }
            else
            {
                model.Imagen_URL = null;
            }

            var (exito, error) = await _articuloService.CrearAsync(model, userId!);

            if (!exito)
            {
                EliminarImagenFisica(model.Imagen_URL);
                ModelState.AddModelError("", error!);
                return View(model);
            }

            TempData["ExitoArticulo"] = "Artículo creado correctamente.";
            return RedirectToAction("Admin");
        }


        [HttpGet("editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Editar(int id)
        {
            var articulo = _articuloService.ObtenerPorId(id);
            if (articulo == null) return NotFound();
            return View(articulo);
        }

        [HttpPost("editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, ArticuloSalud model)
        {
            if (id != model.Articulo_Id) return NotFound();

            ModelState.Remove("Usuario_Id");
            ModelState.Remove("Usuario");
            ModelState.Remove("Usuario.Nombre");
            ModelState.Remove("Usuario.Primer_Apellido");
            ModelState.Remove("Usuario.Segundo_Apellido");
            ModelState.Remove("Fecha_Creacion");
            ModelState.Remove("Fecha_Actualizacion");

            if (!ModelState.IsValid) return View(model);

            model.Usuario = null;
            model.Comentarios = new();
            model.Likes = new();

            var existente = _articuloService.ObtenerPorId(id);
            if (existente == null) return NotFound();
            var imagenAnterior = existente.Imagen_URL;
            model.Imagen_URL = imagenAnterior;

            string? imagenNueva = null;
            if (model.Imagen_Archivo != null && model.Imagen_Archivo.Length > 0)
            {
                imagenNueva = await GuardarImagenAsync(model.Imagen_Archivo);
                if (imagenNueva == null)
                {
                    model.Imagen_URL = imagenAnterior;
                    return View(model);
                }
                model.Imagen_URL = imagenNueva;
            }

            var (exito, error) = await _articuloService.ActualizarAsync(model);
            if (!exito)
            {
                EliminarImagenFisica(imagenNueva);
                model.Imagen_URL = imagenAnterior;
                ModelState.AddModelError("", error!);
                return View(model);
            }

            if (imagenNueva != null)
                EliminarImagenFisica(imagenAnterior);

            TempData["ExitoArticulo"] = "Artículo actualizado correctamente.";
            return RedirectToAction("Admin");
        }

        [HttpPost("eliminar/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var articulo = _articuloService.ObtenerPorId(id);
            var imagen = articulo?.Imagen_URL;

            var (exito, error) = await _articuloService.EliminarAsync(id);
            if (!exito) TempData["ErrorArticulo"] = error;
            else
            {
                EliminarImagenFisica(imagen);
                TempData["ExitoArticulo"] = "Artículo eliminado.";
            }
            return RedirectToAction("Admin");
        }

        [HttpPost("toggle-publicado/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublicado(int id)
        {
            await _articuloService.TogglePublicadoAsync(id);
            return RedirectToAction("Admin");
        }
        private async Task<string?> GuardarImagenAsync(IFormFile archivo)
        {
            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();

            if (!ExtensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError("Imagen_Archivo",
                    "Formato de imagen no permitido. Use JPG, PNG, WEBP o GIF.");
                return null;
            }

            if (archivo.Length > TamanoMaximo)
            {
                ModelState.AddModelError("Imagen_Archivo",
                    "La imagen no puede superar los 5 MB.");
                return null;
            }

            var carpeta = Path.Combine(_env.WebRootPath, CarpetaImagenes);
            Directory.CreateDirectory(carpeta);

            var nombre = $"{Guid.NewGuid():N}{extension}";
            var rutaFisica = Path.Combine(carpeta, nombre);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return $"/{CarpetaImagenes}/{nombre}";
        }

        private void EliminarImagenFisica(string? rutaRelativa)
        {
            if (string.IsNullOrWhiteSpace(rutaRelativa)) return;
            if (!rutaRelativa.StartsWith($"/{CarpetaImagenes}/")) return;

            var rutaFisica = Path.Combine(
                _env.WebRootPath,
                rutaRelativa.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (System.IO.File.Exists(rutaFisica))
                System.IO.File.Delete(rutaFisica);
        }
    }
}