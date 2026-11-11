using System.Globalization;
using Cinemax.Shared.Enums;

namespace Cinemax.Mobile.Services;

// El backend trabaja en UTC; el cine, en hora de Argentina.
public static class ZonaHoraria
{
    private static readonly TimeZoneInfo Zona = ObtenerZona();

    private static TimeZoneInfo ObtenerZona()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
        }
        catch (Exception e) when (e is TimeZoneNotFoundException or InvalidTimeZoneException)
        {
            return TimeZoneInfo.CreateCustomTimeZone("ART", TimeSpan.FromHours(-3), "ART", "ART");
        }
    }

    public static DateTime ALocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zona);

    public static DateTime AUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Zona);

    public static DateOnly HoyLocal() => DateOnly.FromDateTime(ALocal(DateTime.UtcNow));
}

// Textos presentables de los enums (los nombres internos no lo son: mas13, DosD, Frances...).
public static class Etiquetas
{
    public static string DeClasificacion(Clasificacion c) => c switch
    {
        Clasificacion.ATP => "ATP",
        Clasificacion.mas13 => "+13",
        Clasificacion.mas16 => "+16",
        Clasificacion.mas18 => "+18",
        _ => c.ToString()
    };

    public static string DeFormato(FormatoFuncion f) => f switch
    {
        FormatoFuncion.DosD => "2D",
        FormatoFuncion.TresD => "3D",
        FormatoFuncion.CuatroD => "4D",
        _ => f.ToString()
    };

    public static string DeIdioma(IdiomaFuncion i) => i switch
    {
        IdiomaFuncion.Frances => "Francés",
        _ => i.ToString()
    };

    public static string DeEstadoVenta(EstadoVenta e) => e switch
    {
        EstadoVenta.Pendiente => "Pendiente de pago",
        EstadoVenta.Confirmada => "Confirmada",
        EstadoVenta.Cancelada => "Cancelada",
        EstadoVenta.Anulada => "Anulada",
        _ => e.ToString()
    };
}

public static class Formato
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-AR");

    public static string Moneda(decimal valor) => valor.ToString("C2", Cultura);

    public static string Hora(DateTime utc) => ZonaHoraria.ALocal(utc).ToString("HH:mm", Cultura);

    public static string Dia(DateOnly dia) => dia.ToString("ddd dd/MM", Cultura);

    public static string DiaHora(DateTime utc) => ZonaHoraria.ALocal(utc).ToString("ddd dd/MM HH:mm", Cultura);

    public static string Cuenta(TimeSpan t) =>
        t <= TimeSpan.Zero ? "00:00" : $"{(int)t.TotalMinutes:00}:{t.Seconds:00}";
}
