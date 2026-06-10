using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
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

        public PedidoController(
            IPedidoService pedidoService,
            ICarritoService carritoService,
            IFacturaService facturaService,
            UserManager<Usuario> userManager)
        {
            _pedidoService = pedidoService;
            _carritoService = carritoService;
            _facturaService = facturaService;
            _userManager = userManager;
        }

        // ── Cliente: historial ───────────────────────────────────────────────

        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> MisPedidos(
            DateTime? desde, DateTime? hasta, string? estado)
        {
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario?.Cliente == null)
                return RedirectToAction("Index", "Producto");

            var pedidos = _pedidoService
                .ObtenerPorCliente(usuario.Cliente.Cliente_Id);

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
        public async Task<IActionResult> Detalle(int id)
        {
            var pedido = _pedidoService.ObtenerPorId(id);
            if (pedido == null) return NotFound();

            // Verificar que sea el dueño o admin
            if (!User.IsInRole("Admin") && !User.IsInRole("Empleado"))
            {
                var usuario = await _userManager.GetUserAsync(User);
                if (usuario?.Cliente?.Cliente_Id != pedido.Cliente_Id)
                    return Forbid();
            }

            return View(pedido);
        }

        // ── Checkout ─────────────────────────────────────────────────────────

        [HttpGet("checkout")]
        public async Task<IActionResult> Checkout()
        {
            var items = _carritoService.ObtenerCarrito();
            if (!items.Any())
                return RedirectToAction("Index", "Carrito");

            var usuario = await _userManager.GetUserAsync(User);

            var model = new CheckoutViewModel
            {
                Items = items,
                Telefono_Contacto = usuario?.Telefono ?? ""
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

            if (model.Tipo_Entrega == "Domicilio" &&
                string.IsNullOrWhiteSpace(model.Direccion_Entrega))
            {
                ModelState.AddModelError("Direccion_Entrega",
                    "La dirección es obligatoria para entrega a domicilio.");
            }

            if (!ModelState.IsValid)
                return View(model);

            // Si es domicilio redirigir a pago
            if (model.Tipo_Entrega == "Domicilio")
            {
                TempData["CheckoutJson"] = System.Text.Json.JsonSerializer
                    .Serialize(model);
                return RedirectToAction("Pago");
            }

            // Si es tienda procesar de inmediato
            var usuario = await _userManager.GetUserAsync(User);
            if (usuario?.Cliente == null)
                return RedirectToAction("Index", "Producto");

            var (exito, error, pedido) = await _pedidoService
                .ProcesarPedidoAsync(model, usuario.Cliente.Cliente_Id);

            if (!exito)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            _carritoService.LimpiarCarrito();
            TempData["NumeroOrden"] = pedido!.Numero_Orden;

            return RedirectToAction("Confirmacion",
                new { id = pedido.Pedido_Id });
        }

        [HttpGet("pago")]
        public IActionResult Pago()
        {
            var json = TempData["CheckoutJson"]?.ToString();
            if (string.IsNullOrEmpty(json))
                return RedirectToAction("Checkout");

            TempData.Keep("CheckoutJson");

            var model = System.Text.Json.JsonSerializer
                .Deserialize<CheckoutViewModel>(json)!;
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

            var model = System.Text.Json.JsonSerializer
                .Deserialize<CheckoutViewModel>(json)!;
            model.Items = _carritoService.ObtenerCarrito();
            model.Metodo_Pago = metodoPago;

            if (!model.Items.Any())
                return RedirectToAction("Index", "Carrito");

            var usuario = await _userManager.GetUserAsync(User);
            if (usuario?.Cliente == null)
                return RedirectToAction("Index", "Producto");

            var (exito, error, pedido) = await _pedidoService
                .ProcesarPedidoAsync(model, usuario.Cliente.Cliente_Id);

            if (!exito)
            {
                TempData["Error"] = error;
                TempData["CheckoutJson"] = json;
                return RedirectToAction("Pago");
            }

            _carritoService.LimpiarCarrito();
            TempData["NumeroOrden"] = pedido!.Numero_Orden;
            TempData["MetodoPago"] = metodoPago;
            TempData["TelefonoWsp"] = "50622373040";

            return RedirectToAction("Confirmacion",
                new { id = pedido.Pedido_Id });
        }

        [HttpGet("confirmacion/{id:int}")]
        public IActionResult Confirmacion(int id)
        {
            var pedido = _pedidoService.ObtenerPorId(id);
            if (pedido == null) return NotFound();

            var factura = _facturaService.ObtenerPorPedido(id);
            ViewBag.Factura = factura;
            ViewBag.TelefonoWsp = TempData["TelefonoWsp"] ?? "50622373040";
            ViewBag.MetodoPago = TempData["MetodoPago"];

            return View(pedido);
        }

        // ── Admin ─────────────────────────────────────────────────────────────

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

            return View(pedidos);
        }

        [HttpPost("cambiar-estado")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(
            int pedidoId, string nuevoEstado)
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
    }
}