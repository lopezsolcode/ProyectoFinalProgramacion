using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Paginado.Request.Persona
{
    public record ParametroPaginacionPersonaRequest : ParametroPaginacionRequest
    {
        public string? Buscar { get; init; }
        public string? OrdenarPor { get; init; } // "id", "nombre"
        public string? Direccion { get; init; }  // "asc", "desc"
    }
}
