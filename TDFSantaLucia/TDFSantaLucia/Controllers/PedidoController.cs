using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TDFSantaLucia.Controllers
{
    [Authorize]
    [Route("pedido")]
    public class PedidoController : Controller
    {
        private readonly IPedidoService _pedidoService;
        private readonly ICarritoService _carritoService;
        private readonly IFacturaService _facturaService;
        private readonly UserManager<Usuario> _userManager;
        private readonly AppDbContext _db;
        private readonly IWebHostEnvironment _env;

        public PedidoController(
            IPedidoService pedidoService,
            ICarritoService carritoService,
            IFacturaService facturaService,
            UserManager<Usuario> userManager,
            AppDbContext db,
            IWebHostEnvironment env)
        {
            _pedidoService = pedidoService;
            _carritoService = carritoService;
            _facturaService = facturaService;
            _userManager = userManager;
            _db = db;
            _env = env;
        }

        private async Task<Cliente?> ObtenerClienteAsync()
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario == null) return null;
            return await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuario.Id);
        }

        private async Task<string?> GuardarRecetaAsync(IFormFile? archivo)
        {
            if (archivo == null || archivo.Length == 0) return null;
            if (archivo.Length > 5 * 1024 * 1024) return null;

            var extensionesPermitidas = new[] { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };
            var ext = System.IO.Path.GetExtension(archivo.FileName).ToLower();
            if (!extensionesPermitidas.Contains(ext)) return null;

            var uploadsPath = System.IO.Path.Combine(_env.WebRootPath, "uploads", "recetas");
            System.IO.Directory.CreateDirectory(uploadsPath);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = System.IO.Path.Combine(uploadsPath, fileName);

            await using var stream = new System.IO.FileStream(filePath, System.IO.FileMode.Create);
            await archivo.CopyToAsync(stream);

            return $"/uploads/recetas/{fileName}";
        }

        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> MisPedidos(
            DateTime? desde, DateTime? hasta, string? estado)
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var pedidos = _pedidoService.ObtenerPorCliente(cliente.Cliente_Id);

            if (desde.HasValue)
                pedidos = pedidos
                    .Where(p => p.Fecha_Creacion.Date >= desde.Value.Date)
                    .ToList();

            if (hasta.HasValue)
                pedidos = pedidos
                    .Where(p => p.Fecha_Creacion.Date <= hasta.Value.Date)
                    .ToList();

            if (!string.IsNullOrWhiteSpace(estado))
                pedidos = pedidos.Where(p => p.Estado == estado).ToList();

            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            ViewBag.Estados = new SelectList(new[]
            {
                PedidoEstados.Pendiente,
                PedidoEstados.Aceptado,
                PedidoEstados.EnProceso,
                PedidoEstados.Listo,
                PedidoEstados.EnCamino,
                PedidoEstados.Entregado,
                PedidoEstados.Rechazado,
                PedidoEstados.Cancelado
            }, estado);

            return View(pedidos);
        }

        [HttpGet("detalle/{id:int}")]
        public async Task<IActionResult> Detalle(int id, string? desde)
        {
            var pedido = _pedidoService.ObtenerPorId(id);
            if (pedido == null) return NotFound();

            if (!User.IsInRole("Admin") && !User.IsInRole("Empleado"))
            {
                var cliente = await ObtenerClienteAsync();
                if (cliente?.Cliente_Id != pedido.Cliente_Id)
                    return Forbid();
            }

            ViewBag.Desde = desde ?? "mis-pedidos";
            return View(pedido);
        }

        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var items = _carritoService.ObtenerCarrito();
            if (!items.Any())
                return RedirectToAction("Index", "Carrito");

            var usuario = await _userManager.GetUserAsync(User);
            var cliente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuario!.Id);

            int puntosDisponibles = 0;
            if (cliente != null)
            {
                var puntosService = HttpContext.RequestServices
                    .GetRequiredService<IPuntosService>();
                puntosDisponibles = puntosService
                    .ObtenerPuntosDisponibles(cliente.Cliente_Id);
            }

            var model = new CheckoutViewModel
            {
                Items = items,
                Telefono_Contacto = usuario?.Telefono ?? "",
                Puntos_Disponibles = puntosDisponibles,
                RequiereReceta = items.Any(i => i.Receta)
            };

            return View(model);
        }

        [HttpPost("checkout")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            model.Items = _carritoService.ObtenerCarrito();

            if (!model.Items.Any())
                return RedirectToAction("Index", "Carrito");

            model.RequiereReceta = model.Items.Any(i => i.Receta);

            var clienteParaPuntos = await ObtenerClienteAsync();
            if (clienteParaPuntos != null)
            {
                var puntosService = HttpContext.RequestServices
                    .GetRequiredService<IPuntosService>();
                model.Puntos_Disponibles = puntosService
                    .ObtenerPuntosDisponibles(clienteParaPuntos.Cliente_Id);
            }

            if (model.Tipo_Entrega == "Domicilio" &&
                string.IsNullOrWhiteSpace(model.Direccion_Entrega))
            {
                ModelState.AddModelError("Direccion_Entrega",
                    "La dirección es obligatoria para entrega a domicilio.");
            }

            if (model.RequiereReceta &&
                (model.ArchivoReceta == null || model.ArchivoReceta.Length == 0))
            {
                ModelState.AddModelError("ArchivoReceta",
                    "Debes adjuntar la receta médica para continuar.");
            }

            if (!ModelState.IsValid)
                return View(model);

            string? recetaUrl = await GuardarRecetaAsync(model.ArchivoReceta);

            if (model.Tipo_Entrega == "Domicilio")
            {
                TempData["RecetaUrl"] = recetaUrl;
                TempData["CheckoutJson"] = System.Text.Json.JsonSerializer
                    .Serialize(new CheckoutSerializable(model));
                return RedirectToAction("Pago");
            }

            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var (exito, error, pedido) = await _pedidoService
                .ProcesarPedidoAsync(model, cliente.Cliente_Id);

            if (!exito)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            if (!string.IsNullOrEmpty(recetaUrl) && pedido != null)
            {
                pedido.Receta_URL = recetaUrl;
                _db.Pedidos.Update(pedido);
                await _db.SaveChangesAsync();
            }

            _carritoService.LimpiarCarrito();
            TempData["NumeroOrden"] = pedido!.Numero_Orden;

            return RedirectToAction("Confirmacion", new { id = pedido.Pedido_Id });
        }

        [HttpGet("pago")]
        public IActionResult Pago()
        {
            var json = TempData["CheckoutJson"]?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Checkout");

            TempData.Keep("CheckoutJson");
            TempData.Keep("RecetaUrl");

            var serializable = System.Text.Json.JsonSerializer
                .Deserialize<CheckoutSerializable>(json)!;
            var model = serializable.ToViewModel();
            model.Items = _carritoService.ObtenerCarrito();

            return View(model);
        }

        [HttpPost("pago")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pago(string metodoPago)
        {
            var json = TempData["CheckoutJson"]?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Checkout");

            var recetaUrl = TempData["RecetaUrl"]?.ToString();

            var serializable = System.Text.Json.JsonSerializer
                .Deserialize<CheckoutSerializable>(json)!;
            var model = serializable.ToViewModel();
            model.Items = _carritoService.ObtenerCarrito();
            model.Metodo_Pago = metodoPago;

            if (!model.Items.Any())
                return RedirectToAction("Index", "Carrito");

            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var (exito, error, pedido) = await _pedidoService
                .ProcesarPedidoAsync(model, cliente.Cliente_Id);

            if (!exito)
            {
                TempData["Error"] = error;
                TempData["CheckoutJson"] = json;
                TempData["RecetaUrl"] = recetaUrl;
                return RedirectToAction("Pago");
            }

            if (!string.IsNullOrEmpty(recetaUrl) && pedido != null)
            {
                pedido.Receta_URL = recetaUrl;
                _db.Pedidos.Update(pedido);
                await _db.SaveChangesAsync();
            }

            _carritoService.LimpiarCarrito();
            TempData["NumeroOrden"] = pedido!.Numero_Orden;
            TempData["MetodoPago"] = metodoPago;
            TempData["TelefonoWsp"] = "50684659956";

            return RedirectToAction("Confirmacion", new { id = pedido.Pedido_Id });
        }

        [HttpGet("confirmacion/{id:int}")]
        public IActionResult Confirmacion(int id)
        {
            var pedido = _pedidoService.ObtenerPorId(id);
            if (pedido == null) return NotFound();

            var factura = _facturaService.ObtenerPorPedido(id);
            ViewBag.Factura = factura;
            ViewBag.TelefonoWsp = TempData["TelefonoWsp"] ?? "50684659956";
            ViewBag.MetodoPago = TempData["MetodoPago"];

            return View(pedido);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Admin(string? estado, DateTime? desde, DateTime? hasta)
        {
            var pedidos = _pedidoService.ObtenerTodos();

            if (!string.IsNullOrWhiteSpace(estado))
                pedidos = pedidos.Where(p => p.Estado == estado).ToList();

            if (desde.HasValue)
                pedidos = pedidos
                    .Where(p => p.Fecha_Creacion.Date >= desde.Value.Date)
                    .ToList();

            if (hasta.HasValue)
                pedidos = pedidos
                    .Where(p => p.Fecha_Creacion.Date <= hasta.Value.Date)
                    .ToList();

            ViewBag.EstadoFiltro = estado;
            ViewBag.Desde = desde?.ToString("yyyy-MM-dd");
            ViewBag.Hasta = hasta?.ToString("yyyy-MM-dd");

            ViewBag.Estados = new SelectList(new[]
            {
                PedidoEstados.Pendiente,
                PedidoEstados.Aceptado,
                PedidoEstados.Rechazado,
                PedidoEstados.EnProceso,
                PedidoEstados.Listo,
                PedidoEstados.EnCamino,
                PedidoEstados.Entregado,
                PedidoEstados.Cancelado
            }, estado);

            return View(pedidos);
        }

        [HttpPost("cambiar-estado")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int pedidoId, string nuevoEstado)
        {
            var (exito, error) = await _pedidoService
                .CambiarEstadoAsync(pedidoId, nuevoEstado);

            if (!exito)
                TempData["Error"] = error;
            else
                TempData["Exito"] = "Estado actualizado correctamente.";

            return RedirectToAction("Admin");
        }

        [HttpPost("eliminar/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var (exito, error) = _pedidoService.EliminarPedido(id);
            if (!exito)
                TempData["Error"] = error;
            else
                TempData["Exito"] = "Pedido eliminado correctamente.";

            return RedirectToAction("Admin");
        }

        [HttpGet("mis-puntos")]
        public async Task<IActionResult> MisPuntos()
        {
            var cliente = await ObtenerClienteAsync();
            if (cliente == null)
                return RedirectToAction("Index", "Producto");

            var puntosService = HttpContext.RequestServices
                .GetRequiredService<IPuntosService>();

            var historial = puntosService.ObtenerHistorial(cliente.Cliente_Id);
            var disponibles = puntosService.ObtenerPuntosDisponibles(cliente.Cliente_Id);

            ViewBag.PuntosDisponibles = disponibles;
            return View(historial);
        }
    }
}