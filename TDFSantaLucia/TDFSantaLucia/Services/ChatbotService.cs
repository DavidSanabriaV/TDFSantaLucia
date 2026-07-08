using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;
using TDFSantaLucia.Repositories;

namespace TDFSantaLucia.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly IChatbotRepository _repo;
        private readonly AppDbContext _db;

        public ChatbotService(IChatbotRepository repo, AppDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        public List<ChatbotOpcion> ObtenerTodas() => _repo.ObtenerTodas();
        public List<ChatbotOpcion> ObtenerActivas() => _repo.ObtenerActivas();
        public ChatbotOpcion? ObtenerPorId(int id) => _repo.ObtenerPorId(id);

        public (bool exito, string? error) Crear(ChatbotOpcion opcion)
        {
            try
            {
                opcion.Fecha_Creacion = DateTime.Now;
                opcion.Fecha_Actualizacion = DateTime.Now;
                _repo.Agregar(opcion);
                return (true, null);
            }
            catch (Exception ex)
            {
                var detalle = ex.InnerException?.InnerException?.Message
                           ?? ex.InnerException?.Message
                           ?? ex.Message;
                return (false, $"Error: {detalle}");
            }
        }
        public bool ExisteTexto(string texto, int? excluirId = null)
    => _repo.ObtenerTodas()
        .Any(o => o.Texto.ToLower() == texto.ToLower()
               && (excluirId == null || o.Opcion_Id != excluirId));

        public bool ExisteOrden(int orden, int? excluirId = null)
            => _repo.ObtenerTodas()
                .Any(o => o.Orden == orden
                       && (excluirId == null || o.Opcion_Id != excluirId));
        public (bool exito, string? error) Actualizar(ChatbotOpcion opcion)
        {
            try
            {
                opcion.Fecha_Actualizacion = DateTime.Now;
                _repo.Actualizar(opcion);
                return (true, null);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public (bool exito, string? error) Eliminar(int id)
        {
            try
            {
                _repo.Eliminar(id);
                return (true, null);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }


        public string? DetectarIntent(string texto)
        {
            var t = NormalizarTexto(texto);

            if (ContienePalabras(t, "pedido", "orden", "compra", "compre",
                "pedi", "ordene", "mis pedidos", "ver pedido"))
                return ChatbotIntents.VerPedidos;

            if (ContienePalabras(t, "cita", "consulta", "agendar", "turno",
                "reserva", "mis citas", "ver cita", "agenda"))
                return ChatbotIntents.VerCitas;

            if (ContienePalabras(t, "factura", "recibo", "pago", "cobro",
                "mis facturas", "ver factura", "boleta", "comprobante"))
                return ChatbotIntents.VerFacturas;

            if (ContienePalabras(t, "carrito", "cart", "carro",
                "agregar", "productos agregados"))
                return ChatbotIntents.VerCarrito;

            if (ContienePalabras(t, "horario", "hora", "abre", "cierra",
                "abierto", "cerrado", "cuando abren", "atienden",
                "horarios", "abrir"))
                return ChatbotIntents.Horario;

            if (ContienePalabras(t, "direccion", "donde", "ubicacion",
                "mapa", "llegar", "como llego", "estan ubicados",
                "donde estan", "ubicados"))
                return ChatbotIntents.Ubicacion;

            if (ContienePalabras(t, "telefono", "contacto", "llamar",
                "whatsapp", "correo", "email", "comunicar", "numero",
                "celular", "wsp"))
                return ChatbotIntents.Contacto;

            return null;
        }

        private string NormalizarTexto(string texto)
        {
            var normalizado = texto.ToLower().Trim();
            normalizado = normalizado
                .Replace("á", "a").Replace("é", "e")
                .Replace("í", "i").Replace("ó", "o")
                .Replace("ú", "u").Replace("ü", "u")
                .Replace("ñ", "n").Replace("¿", "")
                .Replace("¡", "").Replace("?", "")
                .Replace("!", "").Replace(",", "")
                .Replace(".", "");
            return normalizado;
        }

        private bool ContienePalabras(string texto, params string[] palabras)
            => palabras.Any(p => texto.Contains(p));

        public async Task<ChatbotRespuesta> ResponderAsync(
            int? opcionId, string? textoLibre, string? usuarioId)
        {
            string? intent = null;
            string? urlRedireccion = null;

            if (opcionId.HasValue)
            {
                var opcion = _repo.ObtenerPorId(opcionId.Value);
                if (opcion != null)
                {
                    intent = opcion.Intent;
                    urlRedireccion = opcion.Url_Redireccion;

                    if (!string.IsNullOrEmpty(urlRedireccion))
                        return new ChatbotRespuesta
                        {
                            Tipo = "redireccion",
                            Mensaje = opcion.Respuesta
                                       ?? "Te redirigimos ahora...",
                            Datos = urlRedireccion
                        };

                    if (string.IsNullOrEmpty(intent))
                        return new ChatbotRespuesta
                        {
                            Tipo = "texto",
                            Mensaje = opcion.Respuesta
                                       ?? "Sin respuesta configurada."
                        };
                }
            }

            if (intent == null && !string.IsNullOrWhiteSpace(textoLibre))
                intent = DetectarIntent(textoLibre);

            if (intent == null)
                return RespuestaNoEntendida();

            if (usuarioId == null &&
                intent is ChatbotIntents.VerPedidos
                       or ChatbotIntents.VerCitas
                       or ChatbotIntents.VerFacturas
                       or ChatbotIntents.VerCarrito)
            {
                return new ChatbotRespuesta
                {
                    Tipo = "redireccion",
                    Mensaje = "Para ver esa información necesitás " +
                              "iniciar sesión primero. 🔐 ¿Te llevo al login?",
                    Datos = "/Account/Login"
                };
            }

            return intent switch
            {
                ChatbotIntents.VerPedidos => await ResponderPedidosAsync(usuarioId!),
                ChatbotIntents.VerCitas => await ResponderCitasAsync(usuarioId!),
                ChatbotIntents.VerFacturas => await ResponderFacturasAsync(usuarioId!),
                ChatbotIntents.VerCarrito => await ResponderCarritoAsync(usuarioId!),
                ChatbotIntents.Horario => ResponderHorario(),
                ChatbotIntents.Ubicacion => ResponderUbicacion(),
                ChatbotIntents.Contacto => ResponderContacto(),
                _ => RespuestaNoEntendida()
            };
        }



        private async Task<ChatbotRespuesta> ResponderPedidosAsync(
            string usuarioId)
        {
            var cliente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuarioId);

            if (cliente == null)
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "No encontré tu perfil de cliente."
                };

            var pedidos = await _db.Pedidos
                .Where(p => p.Cliente_Id == cliente.Cliente_Id)
                .OrderByDescending(p => p.Fecha_Creacion)
                .Take(5)
                .Select(p => new
                {
                    p.Numero_Orden,
                    p.Estado,
                    p.Tipo_Entrega,
                    Total = $"₡{p.Total:N2}",
                    Fecha = p.Fecha_Creacion.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            if (!pedidos.Any())
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "No tenés pedidos registrados aún. 🛒"
                };

            return new ChatbotRespuesta
            {
                Tipo = "pedidos",
                Mensaje = $"Tus últimos {pedidos.Count} pedido(s):",
                Datos = pedidos
            };
        }

        private async Task<ChatbotRespuesta> ResponderCitasAsync(
            string usuarioId)
        {
            var cliente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuarioId);

            if (cliente == null)
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "No encontré tu perfil de cliente."
                };

            var citas = await _db.Citas
    .Where(c => c.Cliente_Id == cliente.Cliente_Id &&
                c.Fecha >= DateTime.Today)
    .OrderBy(c => c.Fecha)
    .Take(5)
    .Select(c => new
    {
        Fecha = c.Fecha.ToString("dd/MM/yyyy HH:mm"),
        Servicio = c.Servicio ?? "Sin especificar",
        c.Estado,
        Observaciones = c.Observaciones ?? ""
    })
    .ToListAsync();

            if (!citas.Any())
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "No tenés citas próximas agendadas. 📅"
                };

            return new ChatbotRespuesta
            {
                Tipo = "citas",
                Mensaje = $"Tus próximas {citas.Count} cita(s):",
                Datos = citas
            };
        }

        private async Task<ChatbotRespuesta> ResponderFacturasAsync(
            string usuarioId)
        {
            var cliente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuarioId);

            if (cliente == null)
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "No encontré tu perfil de cliente."
                };

            var facturas = await _db.Facturas
                .Where(f => f.Cliente_Id == cliente.Cliente_Id)
                .OrderByDescending(f => f.Fecha_Emision)
                .Take(5)
                .Select(f => new
                {
                    f.Numero_Factura,
                    f.Estado,
                    Total = $"₡{f.Total:N2}",
                    Fecha = f.Fecha_Emision.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            if (!facturas.Any())
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "No tenés facturas registradas aún. 🧾"
                };

            return new ChatbotRespuesta
            {
                Tipo = "facturas",
                Mensaje = $"Tus últimas {facturas.Count} factura(s):",
                Datos = facturas
            };
        }

        private async Task<ChatbotRespuesta> ResponderCarritoAsync(
            string usuarioId)
        {
            var cliente = await _db.Clientes
                .FirstOrDefaultAsync(c => c.Usuario_ID == usuarioId);

            if (cliente == null)
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "No encontré tu perfil de cliente."
                };

            var items = await _db.CarritoItems
                .Where(c => c.Cliente_Id == cliente.Cliente_Id)
                .Select(c => new
                {
                    c.Nombre,
                    c.Cantidad,
                    Precio = $"₡{c.Precio:N2}",
                    Subtotal = $"₡{c.Precio * c.Cantidad:N2}"
                })
                .ToListAsync();

            if (!items.Any())
                return new ChatbotRespuesta
                {
                    Tipo = "texto",
                    Mensaje = "Tu carrito está vacío. 🛒"
                };

            var total = await _db.CarritoItems
                .Where(c => c.Cliente_Id == cliente.Cliente_Id)
                .SumAsync(c => c.Precio * c.Cantidad);

            return new ChatbotRespuesta
            {
                Tipo = "carrito",
                Mensaje = $"Tenés {items.Count} producto(s) " +
                          $"en tu carrito. Total: ₡{total:N2}",
                Datos = items
            };
        }

        private ChatbotRespuesta ResponderHorario() =>
            new()
            {
                Tipo = "texto",
                Mensaje = "🕐 Nuestro horario:\n" +
                          "Lunes a Sábado: 8:00 am – 9:00 pm\n" +
                          "Domingos: 9:00 am – 8:00 pm"
            };

        private ChatbotRespuesta ResponderUbicacion() =>
            new()
            {
                Tipo = "texto",
                Mensaje = "📍 Nos encontramos en:\n" +
                          "Santa Lucía de Barva, Heredia, Costa Rica.\n" +
                          "2V7H+4WM, Jardines de Santa Lucía."
            };

        private ChatbotRespuesta ResponderContacto() =>
            new()
            {
                Tipo = "texto",
                Mensaje = "📞 Podés contactarnos por:\n" +
                          "Teléfono: 2237-3040\n" +
                          "WhatsApp: 8465-9956\n" +
                          "Email: farmasantalucia@ice.co.cr"
            };

        private ChatbotRespuesta RespuestaNoEntendida() =>
            new()
            {
                Tipo = "texto",
                Mensaje = "Lo siento, no entendí tu consulta. 😕\n" +
                          "Podés usar los botones de abajo o escribir " +
                          "palabras como: pedidos, citas, facturas, " +
                          "horario, dirección o contacto."
            };
    }
}