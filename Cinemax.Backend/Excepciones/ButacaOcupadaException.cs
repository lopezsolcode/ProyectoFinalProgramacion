namespace Cinemax.Backend.Excepciones
{
    // Excepciones/ButacaOcupadaException.cs
    public class ButacaOcupadaException : Exception
    {
        public ButacaOcupadaException()
            : base("Alguna de las butacas elegidas ya fue tomada. Elegí otras.") { }
    }
}
