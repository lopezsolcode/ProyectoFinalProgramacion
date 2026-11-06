using Cinemax.Shared.Enums;
namespace Cinemax.Shared.DTO.Pelicula.Request
{
    public record ActualizarPeliculaRequest(
        string Titulo,
        string Sinopsis,
        Clasificacion Clasificacion,
        DateOnly FechaEstreno,
        int DuracionMinutos,
        string? ImagenUrl,
        string? PosterUrl,
        bool EnCartelera,
        List<int> GenerosIds,
        List<int> ActoresIds,
        List<int> DirectoresIds);
}
