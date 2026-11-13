// Utilidades/FechasUtc.cs
using Cinemax.Backend.Excepciones;

public static class FechasUtc
{
    // Para cuerpos de request: exige que la fecha traiga zona (Z u offset).
    public static DateTime Exigir(DateTime fecha)
    {
        if (fecha.Kind == DateTimeKind.Unspecified)
            throw new DatosLlegaronErradosException("La fecha debe enviarse en UTC (con 'Z' al final).");

        return SinSegundos(fecha.ToUniversalTime());
    }
    // Para filtros de query string: sin zona se asume UTC.
    public static DateTime? Normalizar(DateTime? fecha)
    {
        if (fecha is null) return null;
        DateTime f = fecha.Value;
        return f.Kind switch
        {
            DateTimeKind.Unspecified => DateTime.SpecifyKind(f, DateTimeKind.Utc),
            _ => f.ToUniversalTime()
        };
    }

    private static DateTime SinSegundos(DateTime f) =>
        new(f.Ticks - f.Ticks % TimeSpan.TicksPerMinute, DateTimeKind.Utc);
}