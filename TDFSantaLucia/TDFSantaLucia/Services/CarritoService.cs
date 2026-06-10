using System.Text.Json;
using TDFSantaLucia.Models;

namespace TDFSantaLucia.Services
{
    public class CarritoService : ICarritoService
    {
        private readonly IHttpContextAccessor _httpContext;
        private const string SessionKey = "Carrito";

        public CarritoService(IHttpContextAccessor httpContext)
        {
            _httpContext = httpContext;
        }

        private List<CarritoItem> Leer()
        {
            var json = _httpContext.HttpContext?.Session.GetString(SessionKey);
            if (string.IsNullOrEmpty(json)) return new List<CarritoItem>();
            return JsonSerializer.Deserialize<List<CarritoItem>>(json) ?? new List<CarritoItem>();
        }

        private void Guardar(List<CarritoItem> items)
        {
            var json = JsonSerializer.Serialize(items);
            _httpContext.HttpContext?.Session.SetString(SessionKey, json);
        }

        public List<CarritoItem> ObtenerCarrito() => Leer();

        public void AgregarItem(CarritoItem item)
        {
            var carrito = Leer();
            var existente = carrito.FirstOrDefault(i => i.Producto_Id == item.Producto_Id);

            if (existente != null)
                existente.Cantidad += item.Cantidad;
            else
                carrito.Add(item);

            Guardar(carrito);
        }

        public void ActualizarCantidad(int productoId, int cantidad)
        {
            var carrito = Leer();
            var item = carrito.FirstOrDefault(i => i.Producto_Id == productoId);

            if (item != null)
            {
                if (cantidad <= 0)
                    carrito.Remove(item);
                else
                    item.Cantidad = cantidad;
            }

            Guardar(carrito);
        }

        public void EliminarItem(int productoId)
        {
            var carrito = Leer();
            carrito.RemoveAll(i => i.Producto_Id == productoId);
            Guardar(carrito);
        }

        public void LimpiarCarrito()
        {
            _httpContext.HttpContext?.Session.Remove(SessionKey);
        }

        public int ContarItems()
            => Leer().Sum(i => i.Cantidad);

        public decimal ObtenerTotal()
            => Leer().Sum(i => i.Subtotal);
    }
}