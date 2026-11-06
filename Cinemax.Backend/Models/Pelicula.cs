// Cinemax.Backend/Models/Pelicula.cs
using Cinemax.Backend.Models.Cinemax.Backend.Models;
using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Models
{
    public class Pelicula
    {
        public int Id { get; set; }
        public required string Titulo { get; set; }
        public required string Sinopsis { get; set; }
        public Clasificacion Clasificacion { get; set; }
        public DateOnly FechaEstreno { get; set; }
        public int DuracionMinutos { get; set; }
        public string? ImagenUrl { get; set; }
        public string? PosterUrl { get; set; }
        public bool EnCartelera { get; set; }
        public bool Eliminado { get; set; } = false;

        public ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
        public ICollection<GeneroPelicula> Generos { get; set; } = new List<GeneroPelicula>();
        public ICollection<PersonaPelicula> Personas { get; set; } = new List<PersonaPelicula>();
    }
    public class GeneroPelicula
    {
        public int Id { get; set; }
        public int GeneroId { get; set; }
        public int PeliculaId { get; set; }
        public Genero Genero { get; set; } = null!;
        public Pelicula Pelicula { get; set; } = null!;
    }


}
