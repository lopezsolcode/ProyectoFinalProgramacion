using Cinemax.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cinemax.Shared.DTO.Funcion.Response
{
    public record FuncionResponse(
        int Id, 
        int PeliculaId, 
        string PeliculaTitulo, 
        int SalaId, 
        int SalaNumero,
        FormatoFuncion Formato, 
        IdiomaFuncion Idioma, 
        DateTime FechaHora, 
        DateTime FechaFin, 
        decimal PrecioEntrada);

    public record FuncionCatalogoResponse(
        int Id, 
        int PeliculaId, 
        int SalaNumero,
        FormatoFuncion Formato, 
        IdiomaFuncion Idioma, 
        DateTime FechaHora, 
        decimal PrecioEntrada);

    public record ButacaFuncionResponse(
        int Id, 
        string Fila, 
        int Numero, 
        int Columna, 
        bool Accesible, 
        bool Ocupada);

    public record MapaFuncionResponse(
        int FuncionId, 
        int PeliculaId, 
        string PeliculaTitulo, 
        int SalaNumero,
        FormatoFuncion Formato, 
        IdiomaFuncion Idioma, 
        DateTime FechaHora, 
        decimal PrecioEntrada,
        int TotalColumnas, 
        List<string> Filas, 
        List<ButacaFuncionResponse> Butacas);
}
