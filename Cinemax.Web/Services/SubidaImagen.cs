using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Comun;
using Microsoft.AspNetCore.Components.Forms;

namespace Cinemax.Web.Services;

// Subida de una imagen (multipart) a cualquiera de los endpoints de api/imagenes.
public static class SubidaImagen
{
    public static async Task<ImagenResponse> Subir(
        AutenticacionService autenticacionService, string ruta, IBrowserFile archivo)
    {
        // El contenido se arma dentro de la fábrica: si hay que reintentar por token vencido, se abre un stream nuevo.
        using HttpResponseMessage respuesta = await autenticacionService.EnviarAutenticado(() =>
        {
            var contenido = new MultipartFormDataContent();
            var contenidoArchivo = new StreamContent(archivo.OpenReadStream(PeliculaService.MaxBytesImagen));

            string tipo = !string.IsNullOrWhiteSpace(archivo.ContentType)
                ? archivo.ContentType
                : Path.GetExtension(archivo.Name).Equals(".png", StringComparison.OrdinalIgnoreCase)
                    ? "image/png"
                    : "image/jpeg";

            contenidoArchivo.Headers.ContentType = new MediaTypeHeaderValue(tipo);
            contenido.Add(contenidoArchivo, "archivo", archivo.Name);

            return new HttpRequestMessage(HttpMethod.Post, ruta) { Content = contenido };
        });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo subir la imagen.");

        return await respuesta.Content.ReadFromJsonAsync<ImagenResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la imagen.");
    }
}
