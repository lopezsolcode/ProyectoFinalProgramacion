using System.Net.Http.Json;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Sala.Request;
using Cinemax.Shared.DTO.Sala.Response;

namespace Cinemax.Web.Services;

public class SalaService
{
    private const int TamanoPagina = 50;
    private const int MaximoPaginas = 50;

    private readonly AutenticacionService _autenticacionService;

    public SalaService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<List<SalaResponse>> ObtenerSalas()
    {
        List<SalaResponse> salas = new();
        int pagina = 1;

        while (pagina <= MaximoPaginas)
        {
            PaginadoResponseDto<SalaResponse> resultado = await ObtenerPagina(pagina);

            // Si el servidor no respetó la página pedida, se corta para no duplicar datos.
            if (resultado.NumeroPagina != pagina) break;

            salas.AddRange(resultado.Datos);

            if (!resultado.TienePaginaPosterior) break;

            pagina++;
        }

        return salas;
    }

    public async Task<MapaSalaResponse> ObtenerMapa(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/salas/{id}/mapa"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo obtener el mapa de la sala.");

        return await respuesta.Content.ReadFromJsonAsync<MapaSalaResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió el mapa de la sala.");
    }

    public async Task Crear(CrearSalaRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/salas")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo crear la sala.");
    }

    public async Task Actualizar(int id, ActualizarSalaRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Put, $"api/salas/{id}")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo modificar la sala.");
    }

    public async Task Eliminar(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/salas/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo eliminar la sala.");
    }

    private async Task<PaginadoResponseDto<SalaResponse>> ObtenerPagina(int pagina)
    {
        string ruta = $"api/salas?NumeroPagina={pagina}&TamanoPagina={TamanoPagina}&OrdenarPor=numero&Direccion=asc";

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, ruta));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudieron obtener las salas.");

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<SalaResponse>>()
               ?? throw new InvalidOperationException("No se pudieron obtener las salas.");
    }
}
