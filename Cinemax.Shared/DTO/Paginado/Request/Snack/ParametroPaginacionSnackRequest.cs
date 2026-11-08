using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Paginado.Request.Snack
{
    public record ParametroPaginacionSnackRequest : ParametroPaginacionRequest
    {
        public string? Buscar { get; init; }
        public bool SoloDisponibles { get; init; }
        public string? OrdenarPor { get; init; } // "id", "nombre", "precio", "stock"
        public string? Direccion { get; init; }  // "asc", "desc"
    }
}
