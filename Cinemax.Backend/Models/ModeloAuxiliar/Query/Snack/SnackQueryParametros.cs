namespace Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack
{
    public record SnackQueryParametros : QueryParametros
    {
        public string? Buscar { get; init; }
        public bool SoloDisponibles { get; init; }
        public string? OrdenarPor { get; init; } = "id"; // "id", "nombre", "precio", "stock"
        public string? Direccion { get; init; }          // "asc", "desc"
    }

    public record ComboQueryParametros : QueryParametros
    {
        public string? Buscar { get; init; }
        public string? OrdenarPor { get; init; } = "id"; // "id", "nombre", "precio"
        public string? Direccion { get; init; }          // "asc", "desc"
    }

    // Para que la venta (paso 4) pida stock sin conocer entidades.
    public record ItemSnackVenta(int? SnackId, int? ComboId, int Cantidad);
}
