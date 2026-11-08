using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Paginado.Request.Funcion
{
    public record ParametroPaginacionFuncionRequest : ParametroPaginacionRequest
    {
        public int? PeliculaId { get; init; }
        public int? SalaId { get; init; }
        public DateTime? Desde { get; init; }   // UTC, inclusivo
        public DateTime? Hasta { get; init; }   // UTC, exclusivo
        public string? OrdenarPor { get; init; } // "fecha", "id"
        public string? Direccion { get; init; }  // "asc", "desc"
    }
}
