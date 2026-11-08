using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Pelicula.Request
{
    public record CrearPeliculaRequest(
        string Titulo, 
        string Sinopsis, 
        Clasificacion Clasificacion,
        DateOnly FechaEstreno, 
        int DuracionMinutos, 
        bool EnCartelera,
        List<int> GenerosIds, 
        List<int> ActoresIds, 
        List<int> DirectoresIds);
}
