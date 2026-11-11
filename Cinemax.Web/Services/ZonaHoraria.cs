namespace Cinemax.Web.Services;

// El backend trabaja en UTC; el personal del cine, en hora de Argentina.
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
            return TimeZoneInfo.Local;
        }
    }

    public static DateTime ALocal(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), Zona);

    // Recibe una fecha y hora "de pared" argentinas y devuelve el instante en UTC (Kind = Utc).
    public static DateTime AUtc(DateTime local) =>
        TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), Zona);

    public static DateOnly HoyLocal() => DateOnly.FromDateTime(ALocal(DateTime.UtcNow));
}
