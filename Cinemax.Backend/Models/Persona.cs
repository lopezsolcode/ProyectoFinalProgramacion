using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Models
{
    // Models/Persona.cs
    namespace Cinemax.Backend.Models
    {
        public class Persona
        {
            public int Id { get; set; }
            public required string Nombre { get; set; }
            public string? ImagenUrl { get; set; }
            public bool Eliminado { get; set; } = false;
            public ICollection<PersonaPelicula> Peliculas { get; set; } = new List<PersonaPelicula>();
        }

        public class PersonaPelicula
        {
            public int Id { get; set; }
            public int PersonaId { get; set; }
            public int PeliculaId { get; set; }
            public RolPelicula Rol { get; set; }
            public Persona Persona { get; set; } = null!;
            public Pelicula Pelicula { get; set; } = null!;
        }
    }
}
