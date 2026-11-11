using System.Net;

namespace Cinemax.Mobile.Services;

// Error de la API con su código HTTP, para que las pantallas puedan distinguir un 404 de un 409.
public class ApiException : InvalidOperationException
{
    public HttpStatusCode Codigo { get; }

    public ApiException(string mensaje, HttpStatusCode codigo) : base(mensaje)
    {
        Codigo = codigo;
    }
}

public static class ErrorApi
{
    public static async Task<ApiException> De(
        HttpResponseMessage respuesta, string porDefecto, string? mensajeProhibido = null)
    {
        if (respuesta.StatusCode == HttpStatusCode.Forbidden && mensajeProhibido is not null)
            return new ApiException(mensajeProhibido, respuesta.StatusCode);

        // El backend devuelve el mensaje como texto (a veces entre comillas, si pasó por el manejador global).
        string mensaje = (await respuesta.Content.ReadAsStringAsync()).Trim().Trim('"');

        // Un cuerpo JSON (errores de validación del framework) no sirve para mostrar.
        if (string.IsNullOrWhiteSpace(mensaje) || mensaje.StartsWith('{') || mensaje.StartsWith('['))
            mensaje = porDefecto;

        return new ApiException(mensaje, respuesta.StatusCode);
    }
}
