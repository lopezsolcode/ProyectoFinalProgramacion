using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Paginado.Request.Sala
{
    public record ParametroPaginacionSalaRequest : ParametroPaginacionRequest
    {
        public string? OrdenarPor { get; init; } // "id", "numero"
        public string? Direccion { get; init; }  // "asc", "desc"
    }
}
