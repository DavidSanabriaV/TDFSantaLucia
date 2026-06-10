namespace TDFSantaLucia.Models
{
    public class CarritoItem
    {
        public int Producto_Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Imagen_URL { get; set; }
        public string? Marca { get; set; }
        public decimal Precio { get; set; }
        public int Cantidad { get; set; }
        public bool Receta { get; set; }
        public decimal Subtotal => Precio * Cantidad;
    }
}