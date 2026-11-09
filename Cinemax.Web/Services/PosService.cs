using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using Cinemax.Shared.DTO.Combo.Response;
using Cinemax.Shared.DTO.Funcion.Response;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Pelicula.Response;
using Cinemax.Shared.DTO.Snack.Response;
using Cinemax.Shared.DTO.Venta.Request;
using Cinemax.Shared.DTO.Venta.Response;

namespace Cinemax.Web.Services;

// Error de negocio devuelto por el backend al registrar la venta (400, 404 o 409).
public class PosVentaException : Exception
{
    public bool EsConflicto { get; }

    public PosVentaException(string mensaje, bool esConflicto) : base(mensaje)
    {
        EsConflicto = esConflicto;
    }
}

public class PosService
{
    // Debe coincidir con Ventas:MaxButacasPorVenta del backend.
    public const int MaxButacasPorVenta = 10;

    private const int TamanoPaginaCatalogo = 12;

    private static readonly TimeZoneInfo Zona = ObtenerZona();

    private readonly AutenticacionService _autenticacionService;

    public PosService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    // ---- Zona horaria: el backend trabaja en UTC, el cajero en hora de Argentina ----

    private static TimeZoneInfo ObtenerZona()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.Local;
        }
    }

    public static DateTime ALocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zona);

    public static DateOnly HoyLocal() => DateOnly.FromDateTime(ALocal(DateTime.UtcNow));

    // ---- Catálogos ----

    public Task<PaginadoResponseDto<PeliculaCatalogoResponse>> ObtenerCartelera(int pagina, string? buscar) =>
        Obtener<PaginadoResponseDto<PeliculaCatalogoResponse>>(
            "api/peliculas/cartelera?" + Query(
                ("NumeroPagina", Num(pagina)),
                ("TamanoPagina", Num(TamanoPaginaCatalogo)),
                ("Buscar", buscar),
                ("OrdenarPor", "titulo"),
                ("Direccion", "asc")),
            "El servidor no devolvió la cartelera.");

    public async Task<List<FuncionCatalogoResponse>> ObtenerFuncionesDelDia(int peliculaId, DateOnly dia)
    {
        DateTime inicioLocal = dia.ToDateTime(TimeOnly.MinValue);
        DateTime desde = TimeZoneInfo.ConvertTimeToUtc(inicioLocal, Zona);
        DateTime hasta = TimeZoneInfo.ConvertTimeToUtc(inicioLocal.AddDays(1), Zona);

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
        Obtener<MapaFuncionResponse>(
            $"api/funciones/{funcionId}/mapa",
            "El servidor no devolvió el mapa de butacas.");

    public Task<PaginadoResponseDto<SnackCatalogoResponse>> ObtenerSnacks(int pagina, string? buscar) =>
        Obtener<PaginadoResponseDto<SnackCatalogoResponse>>(
            "api/snacks/catalogo?" + Query(
                ("NumeroPagina", Num(pagina)),
                ("TamanoPagina", Num(TamanoPaginaCatalogo)),
                ("Buscar", buscar),
                ("OrdenarPor", "nombre"),
                ("Direccion", "asc")),
            "El servidor no devolvió el catálogo de snacks.");

    public Task<PaginadoResponseDto<ComboResponse>> ObtenerCombos(int pagina, string? buscar) =>
        Obtener<PaginadoResponseDto<ComboResponse>>(
            "api/combos?" + Query(
                ("NumeroPagina", Num(pagina)),
                ("TamanoPagina", Num(TamanoPaginaCatalogo)),
                ("Buscar", buscar),
                ("OrdenarPor", "nombre"),
                ("Direccion", "asc")),
            "El servidor no devolvió los combos.");

    // ---- Venta ----

    public async Task<VentaResponse> CrearVenta(CrearVentaPresencialRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Post, "api/ventas/presencial")
            {
                Content = JsonContent.Create(request)
            });

        if (respuesta.IsSuccessStatusCode)
        {
            return await respuesta.Content.ReadFromJsonAsync<VentaResponse>()
                   ?? throw new InvalidOperationException("El servidor no devolvió la venta registrada.");
        }

        string mensaje = (await respuesta.Content.ReadAsStringAsync()).Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(mensaje))
        {
            mensaje = "No se pudo registrar la venta.";
        }

        throw new PosVentaException(mensaje, respuesta.StatusCode == HttpStatusCode.Conflict);
    }

    public async Task<byte[]> ObtenerTicketPdf(int ventaId)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, $"api/ventas/{ventaId}/pdf"));

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadAsByteArrayAsync();
    }

    // ---- Auxiliares ----

    private async Task<T> Obtener<T>(string ruta, string mensajeSiVacio)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(
            () => new HttpRequestMessage(HttpMethod.Get, ruta));

        respuesta.EnsureSuccessStatusCode();

        return await respuesta.Content.ReadFromJsonAsync<T>()
               ?? throw new InvalidOperationException(mensajeSiVacio);
    }

    private static string Num(int valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static string Query(params (string Clave, string? Valor)[] pares) =>
        string.Join("&", pares
            .Where(p => !string.IsNullOrWhiteSpace(p.Valor))
            .Select(p => $"{WebUtility.UrlEncode(p.Clave)}={WebUtility.UrlEncode(p.Valor)}"));
}

public static class PosFormato
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

    public static string Moneda(decimal valor) => valor.ToString("C2", Cultura);

    public static string Hora(DateTime utc) => PosService.ALocal(utc).ToString("HH:mm", Cultura);

    public static string Dia(DateOnly dia) => dia.ToString("ddd dd/MM", Cultura);

    public static string DiaHora(DateTime utc) => PosService.ALocal(utc).ToString("ddd dd/MM HH:mm", Cultura);
}