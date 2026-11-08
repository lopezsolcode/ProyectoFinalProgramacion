// Models/Funcion.cs
using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Models
{
    public class Funcion
    {
        public int Id { get; set; }
        public int SalaId { get; set; }
        public int PeliculaId { get; set; }
        public FormatoFuncion Formato { get; set; }
        public IdiomaFuncion Idioma { get; set; }
        public DateTime FechaHora { get; set; }   // UTC
        public DateTime FechaFin { get; set; }    // UTC: inicio + duración + margen
        public decimal PrecioEntrada { get; set; }
        public bool Eliminado { get; set; } = false;
        public Sala Sala { get; set; } = null!;
        public Pelicula Pelicula { get; set; } = null!;
    }
}
