using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Comun;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Persona.Request;
using Cinemax.Shared.DTO.Persona.Response;
using Microsoft.AspNetCore.Components.Forms;

namespace Cinemax.Web.Services;

public class PersonaService
{
    private readonly AutenticacionService _autenticacionService;

    public PersonaService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<PersonaResponse>> ObtenerPersonas(
        int pagina, int tamano, string? buscar, string? ordenarPor, string? direccion)
    {
        var filtros = new Dictionary<string, string>
        {
            ["NumeroPagina"] = pagina.ToString(CultureInfo.InvariantCulture),
            ["TamanoPagina"] = tamano.ToString(CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrWhiteSpace(buscar)) filtros["Buscar"] = buscar;
        if (!string.IsNullOrWhiteSpace(ordenarPor)) filtros["OrdenarPor"] = ordenarPor;
        if (!string.IsNullOrWhiteSpace(direccion)) filtros["Direccion"] = direccion;

        string query = string.Join("&", filtros.Select(f =>
            $"{WebUtility.UrlEncode(f.Key)}={WebUtility.UrlEncode(f.Value)}"));

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/personas?{query}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener el listado de personas.");

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<PersonaResponse>>()
               ?? throw new InvalidOperationException("El servidor no devolvió el listado de personas.");
    }

    // Búsqueda por nombre para los selectores de actores y directores.
    public async Task<List<PersonaResponse>> Buscar(string texto, int cantidad = 8)
    {
        PaginadoResponseDto<PersonaResponse> resultado =
            await ObtenerPersonas(1, cantidad, texto, "nombre", "asc");

        return resultado.Datos;
    }

    public async Task<PersonaResponse> ObtenerPersona(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/personas/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener la persona.");

        return await respuesta.Content.ReadFromJsonAsync<PersonaResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la persona.");
    }

    public async Task<PersonaResponse> Crear(CrearPersonaRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/personas")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo crear la persona.");

        return await respuesta.Content.ReadFromJsonAsync<PersonaResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió la persona creada.");
    }

    public async Task Actualizar(int id, ActualizarPersonaRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Put, $"api/personas/{id}")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo modificar la persona.");
    }

    public async Task Eliminar(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/personas/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo eliminar la persona.");
    }

    public async Task<ImagenResponse> SubirImagen(int personaId, IBrowserFile archivo)
    {
        // El contenido se arma dentro de la fábrica: si hay que reintentar por token vencido, se abre un stream nuevo.
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
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

            return new HttpRequestMessage(HttpMethod.Post, $"api/imagenes/personas/{personaId}")
            {
                Content = contenido
            };
        });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo subir la imagen.");

        return await respuesta.Content.ReadFromJsonAsync<ImagenResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la imagen.");
    }
}
