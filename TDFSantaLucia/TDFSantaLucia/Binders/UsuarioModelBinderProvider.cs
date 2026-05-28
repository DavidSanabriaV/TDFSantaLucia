using TDFSantaLucia.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TDFSantaLucia.Binders
{
    public class UsuarioModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(Usuario))
            {
                return new UsuarioModelBinder();
            }

            return null;
        }
    }
}