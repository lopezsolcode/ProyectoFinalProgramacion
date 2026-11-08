using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Paginado.Request.Combo
{
    public record ParametroPaginacionComboRequest : ParametroPaginacionRequest
    {
        public string? Buscar { get; init; }
        public string? OrdenarPor { get; init; } // "id", "nombre", "precio"
        public string? Direccion { get; init; }  // "asc", "desc"
    }
}
