using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Combo.Response;
using Cinemax.Shared.DTO.Funcion.Response;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Pelicula.Response;
using Cinemax.Shared.DTO.Snack.Response;

namespace Cinemax.Mobile.Services;

// Todo lo que consulta este servicio es público: los visitantes sin sesión también pueden ver la cartelera.
public class CatalogoService
{
    private const int TamanoPagina = 12;

    private readonly HttpClient _httpClient;

    public CatalogoService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<PaginadoResponseDto<PeliculaCatalogoResponse>> ObtenerCartelera(int pagina, string? buscar) =>
        Obtener<PaginadoResponseDto<PeliculaCatalogoResponse>>(
            "api/peliculas/cartelera?" + Query(
                ("NumeroPagina", Num(pagina)),
                ("TamanoPagina", Num(TamanoPagina)),
                ("Buscar", buscar),
                ("OrdenarPor", "titulo"),
                ("Direccion", "asc")),
            "El servidor no devolvió la cartelera.");

    public Task<PeliculaResponse> ObtenerPelicula(int id) =>
        Obtener<PeliculaResponse>($"api/peliculas/{id}", "El servidor no devolvió los datos de la película.");

    public async Task<List<FuncionCatalogoResponse>> ObtenerFuncionesDelDia(int peliculaId, DateOnly dia)
    {
        DateTime inicioLocal = dia.ToDateTime(TimeOnly.MinValue);
        DateTime desde = ZonaHoraria.AUtc(inicioLocal);
        DateTime hasta = ZonaHoraria.AUtc(inicioLocal.AddDays(1));

        PaginadoResponseDto<FuncionCatalogoResponse> resultado =
            await Obtener<PaginadoResponseDto<FuncionCatalogoResponse>>(
                "api/funciones/disponibles?" + Query(
                    ("PeliculaId", Num(peliculaId)),
                    ("Desde", desde.ToString("o", CultureInfo.InvariantCulture)),
                    ("Hasta", hasta.ToString("o", CultureInfo.InvariantCulture)),
                    ("NumeroPagina", "1"),
                    ("TamanoPagina", "50"),
                    ("OrdenarPor", "fecha"),
                    ("Direccion", "asc")),
                "El servidor no devolvió las funciones.");

        return resultado.Datos;
    }

    public Task<MapaFuncionResponse> ObtenerMapa(int funcionId) =>
        Obtener<MapaFuncionResponse>($"api/funciones/{funcionId}/mapa", "El servidor no devolvió el mapa de butacas.");

    public Task<PaginadoResponseDto<SnackCatalogoResponse>> ObtenerSnacks(int pagina, string? buscar) =>
        Obtener<PaginadoResponseDto<SnackCatalogoResponse>>(
            "api/snacks/catalogo?" + Query(
                ("NumeroPagina", Num(pagina)),
                ("TamanoPagina", Num(TamanoPagina)),
                ("Buscar", buscar),
                ("OrdenarPor", "nombre"),
                ("Direccion", "asc")),
            "El servidor no devolvió el catálogo de snacks.");

    public Task<PaginadoResponseDto<ComboResponse>> ObtenerCombos(int pagina, string? buscar) =>
        Obtener<PaginadoResponseDto<ComboResponse>>(
            "api/combos?" + Query(
                ("NumeroPagina", Num(pagina)),
                ("TamanoPagina", Num(TamanoPagina)),
                ("Buscar", buscar),
                ("OrdenarPor", "nombre"),
                ("Direccion", "asc")),
            "El servidor no devolvió los combos.");

    private async Task<T> Obtener<T>(string ruta, string mensajeSiVacio)
    {
        using HttpResponseMessage respuesta = await _httpClient.GetAsync(ruta);

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo consultar el servidor.");

        return await respuesta.Content.ReadFromJsonAsync<T>()
               ?? throw new InvalidOperationException(mensajeSiVacio);
    }

    private static string Num(int valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static string Query(params (string Clave, string? Valor)[] pares) =>
        string.Join("&", pares
            .Where(p => !string.IsNullOrWhiteSpace(p.Valor))
            .Select(p => $"{WebUtility.UrlEncode(p.Clave)}={WebUtility.UrlEncode(p.Valor)}"));
}
