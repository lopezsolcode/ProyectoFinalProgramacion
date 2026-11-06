namespace Cinemax.Backend.Models.ModeloAuxiliar.Query.Genero
{
    public record GeneroQueryParametros : QueryParametros
    {
        public string? Buscar { get; init; }
        public string? OrdenarPor { get; init; } = "id"; // "id", "nombre"
        public string? Direccion { get; init; }          // "asc", "desc"
    }
}
