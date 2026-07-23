using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TDFSantaLucia.Models;
using TDFSantaLucia.Services;

namespace TDFSantaLucia.Controllers
{
    [Route("chatbot")]
    public class ChatbotController : Controller
    {
        private readonly IChatbotService _chatbotService;
        private readonly UserManager<Usuario> _userManager;

        public ChatbotController(
            IChatbotService chatbotService,
            UserManager<Usuario> userManager)
        {
            _chatbotService = chatbotService;
            _userManager = userManager;
        }
        [HttpGet("opciones")]
        public IActionResult ObtenerOpciones()
        {
            var todas = _chatbotService.ObtenerActivas();

            var rutasAdminExactas = new[]
            {
        "/pedido/admin",
        "/cliente",
        "/empleado",
        "/inventario",
        "/expediente",
        "/factura",
        "/cita",
        "/cupon",
        "/categoria",
        "/chatbot/admin",
        "/salud/admin"
    };

            var prefijosAdmin = new[]
            {
        "/pedido/admin",
        "/chatbot/admin",
        "/salud/admin",
        "/cita/crear",
        "/cita/revisar",
        "/cita/asignar",
        "/cupon/crear",
        "/cupon/editar"
    };

            var rutasCliente = new[]
            {
        "/pedido/mis-pedidos",
        "/pedido/mis-puntos",
        "/pedido/checkout",
        "/factura/mis-facturas",
        "/cita/miscitas",
        "/cita/agendar",
        "/cupon/mis-cupones",
        "/tratamiento",
        "/carrito"
    };

            var esAdmin = User.IsInRole("Admin");
            var esEmpleado = User.IsInRole("Empleado");

            var filtradas = todas.Where(o =>
            {
                if (string.IsNullOrEmpty(o.Url_Redireccion))
                    return true;

                if (esAdmin || esEmpleado)
                {
                    if (rutasCliente.Contains(o.Url_Redireccion))
                        return false;
                    return true;
                }

                if (rutasAdminExactas.Contains(o.Url_Redireccion))
                    return false;

                if (prefijosAdmin.Any(p => o.Url_Redireccion.StartsWith(p)))
                    return false;

                return true;
            }).Select(o => new
            {
                o.Opcion_Id,
                o.Texto,
                o.Icono,
                o.Url_Redireccion,
                tieneIntent = !string.IsNullOrEmpty(o.Intent)
            });

            return Json(filtradas);
        }

        [HttpPost("responder")]
        public async Task<IActionResult> Responder(
            [FromBody] ChatbotMensajeRequest request)
        {
            var usuarioId = _userManager.GetUserId(User);

            var respuesta = await _chatbotService.ResponderAsync(
                request.OpcionId,
                request.Texto,
                usuarioId);

            return Json(respuesta);
        }

        [HttpGet("admin")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Admin()
        {
            var opciones = _chatbotService.ObtenerTodas();
            return View(opciones);
        }

        [HttpGet("admin/crear")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Crear() => View(new ChatbotOpcion());

        [HttpPost("admin/crear")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(ChatbotOpcion model)
        {
            ModelState.Remove("Fecha_Creacion");
            ModelState.Remove("Fecha_Actualizacion");
            ModelState.Remove("Respuesta");

            if (string.IsNullOrWhiteSpace(model.Texto))
                ModelState.AddModelError("Texto", "El texto del botón es obligatorio.");
            else if (_chatbotService.ExisteTexto(model.Texto))
                ModelState.AddModelError("Texto",
                    "Ya existe una opción con ese texto.");

            if (string.IsNullOrWhiteSpace(model.Respuesta) &&
                string.IsNullOrWhiteSpace(model.Intent) &&
                string.IsNullOrWhiteSpace(model.Url_Redireccion))
            {
                ModelState.AddModelError("",
                    "Debés completar al menos: una Acción automática, " +
                    "una URL de redirección, o una Respuesta de texto.");
            }

            if (_chatbotService.ExisteOrden(model.Orden))
                ModelState.AddModelError("Orden",
                    $"Ya existe una opción con el orden {model.Orden}. " +
                    "Usá un número diferente.");

            if (!ModelState.IsValid) return View(model);

            var (exito, error) = _chatbotService.Crear(model);
            if (!exito) { ModelState.AddModelError("", error!); return View(model); }

            TempData["ExitoChatbot"] = "Opción creada correctamente.";
            return RedirectToAction("Admin");
        }


        [HttpGet("admin/editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        public IActionResult Editar(int id)
        {
            var opcion = _chatbotService.ObtenerPorId(id);
            if (opcion == null) return NotFound();
            return View(opcion);
        }

        [HttpPost("admin/editar/{id:int}")]
        [Authorize(Roles = "Admin,Empleado")]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(int id, ChatbotOpcion model)
        {
            if (id != model.Opcion_Id) return NotFound();
            ModelState.Remove("Fecha_Creacion");
            ModelState.Remove("Fecha_Actualizacion");
            ModelState.Remove("Respuesta");

            if (string.IsNullOrWhiteSpace(model.Respuesta) &&
                string.IsNullOrWhiteSpace(model.Intent) &&
                string.IsNullOrWhiteSpace(model.Url_Redireccion))
            {
                ModelState.AddModelError("",
                    "Debés completar al menos: una Acción automática, " +
                    "una URL de redirección, o una Respuesta de texto.");
            }
            if (string.IsNullOrWhiteSpace(model.Texto))
                ModelState.AddModelError("Texto", "El texto del botón es obligatorio.");

            else if (_chatbotService.ExisteTexto(model.Texto, model.Opcion_Id))
                ModelState.AddModelError("Texto",
                    "Ya existe una opción con ese texto.");

            if (_chatbotService.ExisteOrden(model.Orden, model.Opcion_Id))
                ModelState.AddModelError("Orden",
                    $"Ya existe una opción con el orden {model.Orden}. " +
                    "Usá un número diferente.");

            if (!ModelState.IsValid) return View(model);

            var (exito, error) = _chatbotService.Actualizar(model);
            if (!exito)
            {
                ModelState.AddModelError("", error!);
                return View(model);
            }

            TempData["ExitoChatbot"] = "Opción actualizada correctamente.";
            return RedirectToAction("Admin");
        }

        [HttpPost("admin/eliminar/{id:int}")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var (exito, error) = _chatbotService.Eliminar(id);
            if (!exito) TempData["ErrorChatbot"] = error;
            else TempData["ExitoChatbot"] = "Opción eliminada.";
            return RedirectToAction("Admin");
        }
    }

    public class ChatbotMensajeRequest
    {
        public int? OpcionId { get; set; }
        public string? Texto { get; set; }
    }
}