namespace Cinemax.Backend.Models.ModeloAuxiliar.Query.Funcion
{
    // Models/ModeloAuxiliar/Query/Funcion/FuncionQueryParametros.cs
    public record FuncionQueryParametros : QueryParametros
    {
        public int? PeliculaId { get; init; }
        public int? SalaId { get; init; }
        public DateTime? Desde { get; init; }   // UTC, inclusivo
        public DateTime? Hasta { get; init; }   // UTC, exclusivo
        public bool SoloFuturas { get; init; }
        public string? OrdenarPor { get; init; } = "fecha"; // "fecha", "id"
        public string? Direccion { get; init; }             // "asc" (por defecto), "desc"
    }
}
