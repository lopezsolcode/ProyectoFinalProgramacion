namespace Cinemax.Backend.Models.ModeloAuxiliar.Query
{
    public record PeliculaQueryParametros : QueryParametros
    {
        public string? Buscar { get; init; }          // por título
        public int? GeneroId { get; init; }
        public bool? EnCartelera { get; init; }
        public string? OrdenarPor { get; init; } = "id"; // "id", "titulo", "estreno", "duracion"
        public string? Direccion { get; init; }          // "asc", "desc"
    }
}
