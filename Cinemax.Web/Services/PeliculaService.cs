using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Comun;
using Cinemax.Shared.DTO.Paginado.Request.Pelicula;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Pelicula.Request;
using Cinemax.Shared.DTO.Pelicula.Response;
using Microsoft.AspNetCore.Components.Forms;

namespace Cinemax.Web.Services;

public class PeliculaService
{
    public const long MaxBytesImagen = 5 * 1024 * 1024;

    private readonly AutenticacionService _autenticacionService;

    public PeliculaService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<PeliculaListadoResponse>> ObtenerPeliculas(
        ParametroPaginacionPeliculaRequest parametros)
    {
        var filtros = new Dictionary<string, string>
        {
            ["NumeroPagina"] = (parametros.NumeroPagina ?? 1).ToString(CultureInfo.InvariantCulture),
            ["TamanoPagina"] = (parametros.TamanoPagina ?? 10).ToString(CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrWhiteSpace(parametros.Buscar))
            filtros["Buscar"] = parametros.Buscar;

        if (parametros.GeneroId.HasValue)
            filtros["GeneroId"] = parametros.GeneroId.Value.ToString(CultureInfo.InvariantCulture);

        if (parametros.EnCartelera.HasValue)
            filtros["EnCartelera"] = parametros.EnCartelera.Value ? "true" : "false";

        if (!string.IsNullOrWhiteSpace(parametros.OrdenarPor))
            filtros["OrdenarPor"] = parametros.OrdenarPor;

        if (!string.IsNullOrWhiteSpace(parametros.Direccion))
            filtros["Direccion"] = parametros.Direccion;

        string query = string.Join("&", filtros.Select(f =>
            $"{WebUtility.UrlEncode(f.Key)}={WebUtility.UrlEncode(f.Value)}"));

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/peliculas?{query}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorDe(respuesta, "No se pudo obtener el listado de películas.");

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<PeliculaListadoResponse>>()
               ?? throw new InvalidOperationException("El servidor no devolvió el listado de películas.");
    }

    public async Task<PeliculaResponse> ObtenerPelicula(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/peliculas/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorDe(respuesta, "No se pudo obtener la película.");

        return await respuesta.Content.ReadFromJsonAsync<PeliculaResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la película.");
    }

    public async Task<PeliculaResponse> Crear(CrearPeliculaRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/peliculas")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorDe(respuesta, "No se pudo crear la película.");

        return await respuesta.Content.ReadFromJsonAsync<PeliculaResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió la película creada.");
    }

    public async Task Actualizar(int id, ActualizarPeliculaRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Put, $"api/peliculas/{id}")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorDe(respuesta, "No se pudo modificar la película.");
    }

    public async Task Eliminar(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/peliculas/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorDe(respuesta, "No se pudo eliminar la película.");
    }

    public Task<ImagenResponse> SubirImagenPrincipal(int peliculaId, IBrowserFile archivo) =>
        SubirImagen($"api/imagenes/peliculas/{peliculaId}/principal", archivo);

    public Task<ImagenResponse> SubirPoster(int peliculaId, IBrowserFile archivo) =>
        SubirImagen($"api/imagenes/peliculas/{peliculaId}/poster", archivo);

    private async Task<ImagenResponse> SubirImagen(string ruta, IBrowserFile archivo)
    {
        // El contenido se arma dentro de la fábrica: si hay que reintentar por token vencido, se abre un stream nuevo.
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
        {
            var contenido = new MultipartFormDataContent();
            var contenidoArchivo = new StreamContent(archivo.OpenReadStream(MaxBytesImagen));

            contenidoArchivo.Headers.ContentType = new MediaTypeHeaderValue(TipoContenido(archivo));
            contenido.Add(contenidoArchivo, "archivo", archivo.Name);

            return new HttpRequestMessage(HttpMethod.Post, ruta) { Content = contenido };
        });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorDe(respuesta, "No se pudo subir la imagen.");

        return await respuesta.Content.ReadFromJsonAsync<ImagenResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la imagen.");
    }

    private static string TipoContenido(IBrowserFile archivo)
    {
        if (!string.IsNullOrWhiteSpace(archivo.ContentType))
            return archivo.ContentType;

        return Path.GetExtension(archivo.Name).Equals(".png", StringComparison.OrdinalIgnoreCase)
            ? "image/png"
            : "image/jpeg";
    }

    private static async Task<InvalidOperationException> ErrorDe(HttpResponseMessage respuesta, string porDefecto)
    {
        string mensaje = (await respuesta.Content.ReadAsStringAsync()).Trim().Trim('"');

        // Si el cuerpo es un JSON (por ejemplo, errores de validación del framework), no sirve para mostrar.
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje.StartsWith('{') || mensaje.StartsWith('['))
            mensaje = porDefecto;

        return new InvalidOperationException(mensaje);
    }
}
