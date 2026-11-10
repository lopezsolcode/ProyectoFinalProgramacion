namespace Cinemax.Web.Services;

public static class ErrorApi
{
    // Convierte una respuesta de error del backend en una excepción con un mensaje mostrable al usuario.
    public static async Task<InvalidOperationException> De(HttpResponseMessage respuesta, string porDefecto)
    {
        string mensaje = (await respuesta.Content.ReadAsStringAsync()).Trim().Trim('"');

        // Si el cuerpo es un JSON (por ejemplo, errores de validación del framework), no sirve para mostrar.
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje.StartsWith('{') || mensaje.StartsWith('['))
            mensaje = porDefecto;

        return new InvalidOperationException(mensaje);
    }
}
