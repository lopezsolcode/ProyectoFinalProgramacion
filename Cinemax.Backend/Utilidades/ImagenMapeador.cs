// Utilidades/ImagenMapeador.cs
using Cinemax.Backend.Models;
using Cinemax.Shared.DTO.Comun;

namespace Cinemax.Backend.Utilidades
{
    public class ImagenMapeador
    {
        private readonly IHttpContextAccessor _http;
        public ImagenMapeador(IHttpContextAccessor http) => _http = http;

        public ImagenResponse? Mapear(Imagen? imagen)
        {
            if (imagen is null) return null;

            HttpRequest? req = _http.HttpContext?.Request;
            string baseUrl = req is null ? "" : $"{req.Scheme}://{req.Host}";

            return new ImagenResponse(
                imagen.Id,
                imagen.NombreOriginal,
                $"{baseUrl}/{imagen.RutaRelativa}",
                imagen.TipoContenido,
                imagen.TamanoBytes,
                imagen.FechaCreacion);
        }
    }
}