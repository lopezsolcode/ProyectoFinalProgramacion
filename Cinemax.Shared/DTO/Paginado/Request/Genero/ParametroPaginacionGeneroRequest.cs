using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Paginado.Request.Genero
{
    public record ParametroPaginacionGeneroRequest : ParametroPaginacionRequest
    {
        public string? Buscar { get; init; }
        public string? OrdenarPor { get; init; } // "id", "nombre"
        public string? Direccion { get; init; }  // "asc", "desc"
    }
}
