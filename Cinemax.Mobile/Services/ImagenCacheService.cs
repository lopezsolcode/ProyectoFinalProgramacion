namespace Cinemax.Mobile.Services;

// En Android con la API local (certificado de desarrollo autofirmado), el WebView rechaza las imágenes
// aunque el HttpClient sí las acepte. Por eso, solo en ese caso, se bajan con el HttpClient y se
// muestran como data URI. En el resto de los casos (incluida la versión publicada) se usa la URL tal cual.
public class ImagenCacheService
{
#if DEBUG && ANDROID
    private readonly HttpClient _httpClient;
    private readonly Dictionary<string, string> _cache = new();

    public ImagenCacheService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> ObtenerSrc(string url)
    {
        if (_cache.TryGetValue(url, out string? existente)) return existente;

        try
        {
            using HttpResponseMessage respuesta = await _httpClient.GetAsync(url);

            if (!respuesta.IsSuccessStatusCode) return null;

            byte[] bytes = await respuesta.Content.ReadAsByteArrayAsync();
            string tipo = respuesta.Content.Headers.ContentType?.MediaType ?? "image/jpeg";
            string src = $"data:{tipo};base64,{Convert.ToBase64String(bytes)}";

            _cache[url] = src;
            return src;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
#else
    public Task<string?> ObtenerSrc(string url) => Task.FromResult<string?>(url);
#endif
}
