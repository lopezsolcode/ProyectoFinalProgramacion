using Cinemax.Backend.Excepciones;

namespace Cinemax.Backend.Utilidades
{
    // Utilidades/ValidacionUrl.cs
    public static class ValidacionUrl
    {
        public static void ExigirOpcionalHttp(string? url, string campo)
        {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (url.Trim().Length > 500)
                throw new DatosLlegaronErradosException($"La URL de {campo} no puede superar 500 caracteres.");

            bool valida = Uri.TryCreate(url.Trim(), UriKind.Absolute, out Uri? uri)
                          && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
            if (!valida)
                throw new DatosLlegaronErradosException($"La URL de {campo} no es válida.");
        }
    }
}
