using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Comun;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Snack.Request;
using Cinemax.Shared.DTO.Snack.Response;
using Microsoft.AspNetCore.Components.Forms;

namespace Cinemax.Web.Services;

public static class SnackResponseExtensiones
{
    // El stock es el último valor de SnackResponse (Id, Nombre, Precio, Imagen, DescripcionUnidad, stock).
    // Se lee por posición para no depender del nombre de ese parámetro; si prefieres, cámbialo por la propiedad.
    public static int StockActual(this SnackResponse snack)
    {
        var (_, _, _, _, _, stock) = snack;
        return stock;
    }
}

public class SnackService
{
    private readonly AutenticacionService _autenticacionService;

    public SnackService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<PaginadoResponseDto<SnackResponse>> ObtenerSnacks(
        int pagina, int tamano, string? buscar, bool soloDisponibles, string? ordenarPor, string? direccion)
    {
        var filtros = new Dictionary<string, string>
        {
            ["NumeroPagina"] = pagina.ToString(CultureInfo.InvariantCulture),
            ["TamanoPagina"] = tamano.ToString(CultureInfo.InvariantCulture)
        };

        if (!string.IsNullOrWhiteSpace(buscar)) filtros["Buscar"] = buscar;
        if (soloDisponibles) filtros["SoloDisponibles"] = "true";
        if (!string.IsNullOrWhiteSpace(ordenarPor)) filtros["OrdenarPor"] = ordenarPor;
        if (!string.IsNullOrWhiteSpace(direccion)) filtros["Direccion"] = direccion;

        string query = string.Join("&", filtros.Select(f =>
            $"{WebUtility.UrlEncode(f.Key)}={WebUtility.UrlEncode(f.Value)}"));

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/snacks?{query}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener el listado de snacks.");

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<SnackResponse>>()
               ?? throw new InvalidOperationException("El servidor no devolvió el listado de snacks.");
    }

    // Todos los snacks (en páginas de 50), para elegir los componentes de un combo.
    public async Task<List<SnackResponse>> ObtenerTodosParaSelector()
    {
        List<SnackResponse> todos = new();

        for (int pagina = 1; pagina <= 20; pagina++)
        {
            PaginadoResponseDto<SnackResponse> resultado =
                await ObtenerSnacks(pagina, 50, null, false, "nombre", "asc");

            // Si el servidor no respetó la página pedida, se corta para no duplicar datos.
            if (resultado.NumeroPagina != pagina) break;

            todos.AddRange(resultado.Datos);

            if (!resultado.TienePaginaPosterior) break;
        }

        return todos;
    }

    public async Task<SnackResponse> ObtenerSnack(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/snacks/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener el snack.");

        return await respuesta.Content.ReadFromJsonAsync<SnackResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos del snack.");
    }

    public async Task<SnackResponse> Crear(CrearSnackRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/snacks")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo crear el snack.");

        return await respuesta.Content.ReadFromJsonAsync<SnackResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió el snack creado.");
    }

    public async Task Actualizar(int id, ActualizarSnackRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Put, $"api/snacks/{id}")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo modificar el snack.");
    }

    // Suma o resta unidades al stock. El backend rechaza un ajuste que lo deje en negativo.
    public async Task<SnackResponse> AjustarStock(int id, AjustarStockRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, $"api/snacks/{id}/stock")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo ajustar el stock.");

        return await respuesta.Content.ReadFromJsonAsync<SnackResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió el snack actualizado.");
    }

    public async Task Eliminar(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/snacks/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo eliminar el snack.");
    }

    public Task<ImagenResponse> SubirImagen(int snackId, IBrowserFile archivo) =>
        SubidaImagen.Subir(_autenticacionService, $"api/imagenes/snacks/{snackId}", archivo);
}
