using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Funcion.Request;
using Cinemax.Shared.DTO.Funcion.Response;
using Cinemax.Shared.DTO.Paginado.Response;

namespace Cinemax.Web.Services;

public class FuncionService
{
    private readonly AutenticacionService _autenticacionService;

    public FuncionService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    // Listado de administración: incluye funciones pasadas. Desde y Hasta son instantes UTC.
    public async Task<PaginadoResponseDto<FuncionResponse>> ObtenerFunciones(
        int pagina, int tamano, int? peliculaId, int? salaId,
        DateTime? desdeUtc, DateTime? hastaUtc, string? ordenarPor, string? direccion)
    {
        var filtros = new Dictionary<string, string>
        {
            ["NumeroPagina"] = pagina.ToString(CultureInfo.InvariantCulture),
            ["TamanoPagina"] = tamano.ToString(CultureInfo.InvariantCulture)
        };

        if (peliculaId.HasValue) filtros["PeliculaId"] = peliculaId.Value.ToString(CultureInfo.InvariantCulture);
        if (salaId.HasValue) filtros["SalaId"] = salaId.Value.ToString(CultureInfo.InvariantCulture);
        if (desdeUtc.HasValue) filtros["Desde"] = desdeUtc.Value.ToString("o", CultureInfo.InvariantCulture);
        if (hastaUtc.HasValue) filtros["Hasta"] = hastaUtc.Value.ToString("o", CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(ordenarPor)) filtros["OrdenarPor"] = ordenarPor;
        if (!string.IsNullOrWhiteSpace(direccion)) filtros["Direccion"] = direccion;

        string query = string.Join("&", filtros.Select(f =>
            $"{WebUtility.UrlEncode(f.Key)}={WebUtility.UrlEncode(f.Value)}"));

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/funciones?{query}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener el listado de funciones.");

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<FuncionResponse>>()
               ?? throw new InvalidOperationException("El servidor no devolvió el listado de funciones.");
    }

    public async Task<FuncionResponse> ObtenerFuncion(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/funciones/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener la función.");

        return await respuesta.Content.ReadFromJsonAsync<FuncionResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la función.");
    }

    public async Task<FuncionResponse> Crear(CrearFuncionRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/funciones")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo crear la función.");

        return await respuesta.Content.ReadFromJsonAsync<FuncionResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió la función creada.");
    }

    public async Task Actualizar(int id, ActualizarFuncionRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Put, $"api/funciones/{id}")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo modificar la función.");
    }

    public async Task Eliminar(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/funciones/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo eliminar la función.");
    }
}
