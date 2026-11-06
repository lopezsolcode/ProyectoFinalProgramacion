namespace Cinemax.Backend.Models
{
    public class Genero
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public bool Eliminado { get; set; } = false;
        public ICollection<GeneroPelicula> Peliculas { get; set; } = new List<GeneroPelicula>();
    }
}
