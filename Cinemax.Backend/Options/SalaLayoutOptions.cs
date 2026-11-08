namespace Cinemax.Backend.Options
{
    public class SalaLayoutOptions
    {
        public const string Seccion = "SalaLayout";

        public int Filas { get; set; }
        // Sin valores por defecto a propósito: el binder de .NET agrega los valores
        // del JSON a los de una lista inicializada en vez de reemplazarlos.
        public List<int> Bloques { get; set; } = new();
        public int AnchoPasillo { get; set; }
        public bool UltimaFilaSinBloqueCentral { get; set; }
        public List<int> UltimaFilaAccesibles { get; set; } = new();
    }
}
