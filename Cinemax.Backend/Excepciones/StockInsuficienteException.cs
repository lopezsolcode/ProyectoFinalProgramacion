namespace Cinemax.Backend.Excepciones
{
    public class StockInsuficienteException : Exception
    {
        public StockInsuficienteException(string mensaje) : base(mensaje) { }
    }
}
