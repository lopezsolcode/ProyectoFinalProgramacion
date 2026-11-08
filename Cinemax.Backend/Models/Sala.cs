namespace Cinemax.Backend.Models
{
    public class Sala
    {
        public int Id { get; set; }
        public int Numero { get; set; }
        public bool Eliminado { get; set; } = false;
        public ICollection<Butaca> Butacas { get; set; } = new List<Butaca>();
        public ICollection<Funcion> Funciones { get; set; } = new List<Funcion>();
    }
    public class Butaca
    {
        public int Id { get; set; }
        public int SalaId { get; set; }
        public required string Fila { get; set; }   // "A", "B"...
        public int Numero { get; set; }             // etiqueta visible (1 a 19)
        public int Columna { get; set; }            // posición en la grilla, contando pasillos
        public bool Accesible { get; set; }
        public Sala Sala { get; set; } = null!;
    }
}
