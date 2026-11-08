using Cinemax.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Funcion.Request
{
    public record ActualizarFuncionRequest(
        int PeliculaId, 
        int SalaId, 
        FormatoFuncion Formato,
        IdiomaFuncion Idioma, 
        DateTime FechaHora, 
        decimal PrecioEntrada);
}
