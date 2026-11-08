namespace Cinemax.Backend.Models.ModeloAuxiliar.Query.Sala
{
    public record SalaQueryParametros : QueryParametros
    {
        public string? OrdenarPor { get; init; } = "id"; // "id", "numero"
        public string? Direccion { get; init; }          // "asc", "desc"
    }
}
