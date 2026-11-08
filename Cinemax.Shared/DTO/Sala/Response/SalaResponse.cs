using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Sala.Response
{
    public record SalaResponse(int Id, int Numero, int CantidadButacas);
    public record ButacaResponse(int Id, string Fila, int Numero, int Columna, bool Accesible);
    public record MapaSalaResponse(int SalaId, int NumeroSala, int TotalColumnas,
        List<string> Filas, List<ButacaResponse> Butacas);
}
