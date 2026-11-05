using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface ITokenService
    {
        public (string token, DateTime expiracion) CrearToken(Usuario usuario);
        public string CrearRefreshToken();
        public string CalcularHash(string token);
    }
}
