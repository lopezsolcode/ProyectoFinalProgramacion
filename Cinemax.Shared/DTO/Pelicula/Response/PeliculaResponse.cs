using Cinemax.Shared.DTO.Comun;
using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Pelicula.Response
{
    public record PeliculaListadoResponse(
        int Id, 
        string Titulo, 
        Clasificacion Clasificacion,
        DateOnly FechaEstreno, 
        int DuracionMinutos, 
        bool EnCartelera);
    public record PeliculaCatalogoResponse(
        int Id, 
        string Titulo, 
        Clasificacion Clasificacion,
        int DuracionMinutos, 
        string? PosterUrl, 
        List<string> Generos);
    public record PeliculaResponse(
        int Id, 
        string Titulo, 
        string Sinopsis, 
        Clasificacion Clasificacion,
        DateOnly FechaEstreno, 
        int DuracionMinutos, 
        string? ImagenUrl, 
        string? PosterUrl, 
        bool EnCartelera,
        List<ReferenciaResponse> Generos, 
        List<ReferenciaResponse> Actores, 
        List<ReferenciaResponse> Directores);
}
