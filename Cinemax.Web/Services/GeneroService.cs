using System.Net.Http.Json;
using Cinemax.Shared.DTO.Genero.Request;
using Cinemax.Shared.DTO.Genero.Response;
using Cinemax.Shared.DTO.Paginado.Response;

namespace Cinemax.Web.Services;

public class GeneroService
{
    private const int TamanoPagina = 50;
    private const int MaximoPaginas = 50;

    private readonly AutenticacionService _autenticacionService;

    public GeneroService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    public async Task<List<GeneroResponse>> ObtenerGeneros()
    {
        List<GeneroResponse> generos = new();
        int pagina = 1;

        while (pagina <= MaximoPaginas)
        {
            PaginadoResponseDto<GeneroResponse> resultado = await ObtenerPagina(pagina);

            // Si el servidor no respetó la página pedida, se corta para no duplicar datos.
            if (resultado.NumeroPagina != pagina) break;

            generos.AddRange(resultado.Datos);

            if (!resultado.TienePaginaPosterior) break;

            pagina++;
        }

        return generos;
    }

    public async Task Crear(CrearGeneroRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/generos")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo crear el género.");
    }

    public async Task Actualizar(int id, ActualizarGeneroRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Put, $"api/generos/{id}")
            {
                Content = JsonContent.Create(request)
            });

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo modificar el género.");
    }

    public async Task Eliminar(int id)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Delete, $"api/generos/{id}"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo eliminar el género.");
    }

    private async Task<PaginadoResponseDto<GeneroResponse>> ObtenerPagina(int pagina)
    {
        string ruta = $"api/generos?NumeroPagina={pagina}&TamanoPagina={TamanoPagina}";

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, ruta));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudieron obtener los géneros.");

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<GeneroResponse>>()
               ?? throw new InvalidOperationException("No se pudieron obtener los géneros.");
    }
}
