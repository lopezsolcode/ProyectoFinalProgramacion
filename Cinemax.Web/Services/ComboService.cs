using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Combo.Request;
using Cinemax.Shared.DTO.Combo.Response;
using Cinemax.Shared.DTO.Comun;
using Cinemax.Shared.DTO.Paginado.Response;
using Microsoft.AspNetCore.Components.Forms;

namespace Cinemax.Web.Services;

public class ComboService
{
    private readonly AutenticacionService _autenticacionService;

    public ComboService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<ComboResponse>> ObtenerCombos(
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
            () => new HttpRequestMessage(HttpMethod.Get, $"api/combos?{query}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener el listado de combos.");

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<ComboResponse>>()
               ?? throw new InvalidOperationException("El servidor no devolvió el listado de combos.");
    }

    public async Task<ComboResponse> ObtenerCombo(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/combos/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener el combo.");

        return await respuesta.Content.ReadFromJsonAsync<ComboResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos del combo.");
    }

    public async Task<ComboResponse> Crear(CrearComboRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/combos")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo crear el combo.");

        return await respuesta.Content.ReadFromJsonAsync<ComboResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió el combo creado.");
    }

    public async Task Actualizar(int id, ActualizarComboRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Put, $"api/combos/{id}")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo modificar el combo.");
    }

    public async Task Eliminar(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/combos/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo eliminar el combo.");
    }

    public Task<ImagenResponse> SubirImagen(int comboId, IBrowserFile archivo) =>
        SubidaImagen.Subir(_autenticacionService, $"api/imagenes/combos/{comboId}", archivo);
}
