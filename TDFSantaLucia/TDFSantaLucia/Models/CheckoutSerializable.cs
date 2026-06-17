namespace TDFSantaLucia.Models
{
    public class CheckoutSerializable
    {
        public string? Tipo_Entrega { get; set; }
        public string? Metodo_Pago { get; set; }
        public string? Direccion_Entrega { get; set; }
        public string? Telefono_Contacto { get; set; }
        public bool RequiereReceta { get; set; }
        public bool Canjear_Puntos { get; set; }
        public int Puntos_Disponibles { get; set; }
        public int Puntos_A_Canjear { get; set; }
        public int? Cupon_Id { get; set; }
        public int ClienteCupon_Id { get; set; }
        public string? Descuento_Cupon_Raw { get; set; }

        public CheckoutSerializable() { }

        public CheckoutSerializable(CheckoutViewModel vm)
        {
            Tipo_Entrega = vm.Tipo_Entrega;
            Metodo_Pago = vm.Metodo_Pago;
            Direccion_Entrega = vm.Direccion_Entrega;
            Telefono_Contacto = vm.Telefono_Contacto;
            RequiereReceta = vm.RequiereReceta;
            Canjear_Puntos = vm.Canjear_Puntos;
            Puntos_Disponibles = vm.Puntos_Disponibles;
            Puntos_A_Canjear = vm.Puntos_A_Canjear;
            Cupon_Id = vm.Cupon_Id;
            ClienteCupon_Id = vm.ClienteCupon_Id;
            Descuento_Cupon_Raw = vm.Descuento_Cupon_Raw;
        }

        public CheckoutViewModel ToViewModel() => new()
        {
            Tipo_Entrega = Tipo_Entrega,
            Metodo_Pago = Metodo_Pago,
            Direccion_Entrega = Direccion_Entrega,
            Telefono_Contacto = Telefono_Contacto,
            RequiereReceta = RequiereReceta,
            Canjear_Puntos = Canjear_Puntos,
            Puntos_Disponibles = Puntos_Disponibles,
            Puntos_A_Canjear = Puntos_A_Canjear,
            Cupon_Id = Cupon_Id,
            ClienteCupon_Id = ClienteCupon_Id,
            Descuento_Cupon_Raw = Descuento_Cupon_Raw
        };
    }
}