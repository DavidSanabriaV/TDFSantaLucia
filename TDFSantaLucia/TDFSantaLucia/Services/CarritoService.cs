using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using TDFSantaLucia.Data;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IHttpContextAccessor _httpContext;
        private readonly AppDbContext _db;
        private const string SessionKeyPrefix = "Carrito_";

        public CarritoService(IHttpContextAccessor httpContext, AppDbContext db)
        {
            _httpContext = httpContext;
            _db = db;
        }

        private string ObtenerClave()
        {
            var userId = _httpContext.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrEmpty(userId)
                ? $"{SessionKeyPrefix}anonimo"
                : $"{SessionKeyPrefix}{userId}";
        }

        private int? ObtenerClienteId()
        {
            var userId = _httpContext.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return null;

            return _db.Clientes
                .Where(c => c.Usuario_ID == userId)
                .Select(c => (int?)c.Cliente_Id)
                .FirstOrDefault();
        }

        private List<CarritoItem> LeerSesion()
        {
            var json = _httpContext.HttpContext?
                .Session.GetString(ObtenerClave());
            if (string.IsNullOrEmpty(json)) return new List<CarritoItem>();
            return JsonSerializer.Deserialize<List<CarritoItem>>(json)
                   ?? new List<CarritoItem>();
        }

        private void GuardarSesion(List<CarritoItem> items)
        {
            var json = JsonSerializer.Serialize(items);
            _httpContext.HttpContext?.Session.SetString(ObtenerClave(), json);
        }

        public List<CarritoItem> ObtenerCarrito()
        {
            var items = LeerSesion();
            if (items.Any()) return items;

            var clienteId = ObtenerClienteId();
            if (clienteId == null) return items;

            var itemsDb = _db.CarritoItems
                .Where(c => c.Cliente_Id == clienteId)
                .ToList();

            if (!itemsDb.Any()) return items;

            items = itemsDb.Select(i => new CarritoItem
            {
                Producto_Id = i.Producto_Id,
                Nombre = i.Nombre,
                Imagen_URL = i.Imagen_URL,
                Marca = i.Marca,
                Precio = i.Precio,
                Cantidad = i.Cantidad,
                Receta = i.Receta
            }).ToList();

            GuardarSesion(items);
            return items;
        }

        public int ContarItems() => ObtenerCarrito().Sum(i => i.Cantidad);
        public decimal ObtenerTotal() => ObtenerCarrito().Sum(i => i.Subtotal);


        public void AgregarItem(CarritoItem item)
            => AgregarItemAsync(item).GetAwaiter().GetResult();

        public void ActualizarCantidad(int productoId, int cantidad)
            => ActualizarCantidadAsync(productoId, cantidad).GetAwaiter().GetResult();

        public void EliminarItem(int productoId)
            => EliminarItemAsync(productoId).GetAwaiter().GetResult();

        public void LimpiarCarrito()
            => LimpiarCarritoAsync().GetAwaiter().GetResult();


        public async Task AgregarItemAsync(CarritoItem item)
        {
            var carrito = LeerSesion();
            var existente = carrito
                .FirstOrDefault(i => i.Producto_Id == item.Producto_Id);

            if (existente != null)
                existente.Cantidad += item.Cantidad;
            else
                carrito.Add(item);

            GuardarSesion(carrito);

            var clienteId = ObtenerClienteId();
            if (clienteId == null) return;

            var itemDb = await _db.CarritoItems
                .FirstOrDefaultAsync(c => c.Cliente_Id == clienteId
                                       && c.Producto_Id == item.Producto_Id);
            if (itemDb != null)
            {
                itemDb.Cantidad += item.Cantidad;
                _db.CarritoItems.Update(itemDb);
            }
            else
            {
                await _db.CarritoItems.AddAsync(new CarritoItemDb
                {
                    Cliente_Id = clienteId.Value,
                    Producto_Id = item.Producto_Id,
                    Nombre = item.Nombre,
                    Imagen_URL = item.Imagen_URL,
                    Marca = item.Marca,
                    Precio = item.Precio,
                    Cantidad = item.Cantidad,
                    Receta = item.Receta
                });
            }

            await _db.SaveChangesAsync();
        }

        public async Task ActualizarCantidadAsync(int productoId, int cantidad)
        {
            var carrito = LeerSesion();
            var item = carrito.FirstOrDefault(i => i.Producto_Id == productoId);
            if (item != null)
            {
                if (cantidad <= 0) carrito.Remove(item);
                else item.Cantidad = cantidad;
            }
            GuardarSesion(carrito);

            var clienteId = ObtenerClienteId();
            if (clienteId == null) return;

            var itemDb = await _db.CarritoItems
                .FirstOrDefaultAsync(c => c.Cliente_Id == clienteId
                                       && c.Producto_Id == productoId);
            if (itemDb == null) return;

            if (cantidad <= 0)
                _db.CarritoItems.Remove(itemDb);
            else
            {
                itemDb.Cantidad = cantidad;
                _db.CarritoItems.Update(itemDb);
            }

            await _db.SaveChangesAsync();
        }

        public async Task EliminarItemAsync(int productoId)
        {
            
            var carrito = LeerSesion();
            carrito.RemoveAll(i => i.Producto_Id == productoId);
            GuardarSesion(carrito);

            var clienteId = ObtenerClienteId();
            if (clienteId == null) return;

            var itemDb = await _db.CarritoItems
                .FirstOrDefaultAsync(c => c.Cliente_Id == clienteId
                                       && c.Producto_Id == productoId);
            if (itemDb != null)
            {
                _db.CarritoItems.Remove(itemDb);
                await _db.SaveChangesAsync();
            }
        }

        public async Task LimpiarCarritoAsync()
        {
            _httpContext.HttpContext?.Session.Remove(ObtenerClave());

            var clienteId = ObtenerClienteId();
            if (clienteId == null) return;

            var items = _db.CarritoItems
                .Where(c => c.Cliente_Id == clienteId);
            _db.CarritoItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }


        public async Task SincronizarSesionADbAsync()
        {
            var clienteId = ObtenerClienteId();
            if (clienteId == null) return;

            var itemsSesion = LeerSesion();

            foreach (var item in itemsSesion)
            {
                var itemDb = await _db.CarritoItems
                    .FirstOrDefaultAsync(c => c.Cliente_Id == clienteId
                                           && c.Producto_Id == item.Producto_Id);
                if (itemDb != null)
                {
                    itemDb.Cantidad += item.Cantidad;
                    _db.CarritoItems.Update(itemDb);
                }
                else
                {
                    await _db.CarritoItems.AddAsync(new CarritoItemDb
                    {
                        Cliente_Id = clienteId.Value,
                        Producto_Id = item.Producto_Id,
                        Nombre = item.Nombre,
                        Imagen_URL = item.Imagen_URL,
                        Marca = item.Marca,
                        Precio = item.Precio,
                        Cantidad = item.Cantidad,
                        Receta = item.Receta
                    });
                }
            }

            await _db.SaveChangesAsync();
            await SincronizarDbASesionAsync();
        }

        public async Task SincronizarDbASesionAsync()
        {
            var clienteId = ObtenerClienteId();
            if (clienteId == null) return;

            var itemsDb = await _db.CarritoItems
                .Where(c => c.Cliente_Id == clienteId)
                .ToListAsync();

            var items = itemsDb.Select(i => new CarritoItem
            {
                Producto_Id = i.Producto_Id,
                Nombre = i.Nombre,
                Imagen_URL = i.Imagen_URL,
                Marca = i.Marca,
                Precio = i.Precio,
                Cantidad = i.Cantidad,
                Receta = i.Receta
            }).ToList();

            GuardarSesion(items);
        }
    }
}