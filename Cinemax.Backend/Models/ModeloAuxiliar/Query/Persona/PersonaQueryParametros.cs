namespace Cinemax.Backend.Models.ModeloAuxiliar.Query.Persona
{
    public record PersonaQueryParametros : QueryParametros
    {
        public string? Buscar { get; init; }
        public string? OrdenarPor { get; init; } = "id"; // "id", "nombre"
        public string? Direccion { get; init; }          // "asc", "desc"
    }
}
