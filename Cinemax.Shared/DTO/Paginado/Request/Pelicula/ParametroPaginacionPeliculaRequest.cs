namespace Cinemax.Shared.DTO.Paginado.Request.Pelicula
{
    public record ParametroPaginacionPeliculaRequest : ParametroPaginacionRequest
    {
        public string? Buscar { get; init; }
        public int? GeneroId { get; init; }
        public bool? EnCartelera { get; init; }
        public string? OrdenarPor { get; init; } // "id", "titulo", "estreno", "duracion"
        public string? Direccion { get; init; }  // "asc", "desc"
    }
}
