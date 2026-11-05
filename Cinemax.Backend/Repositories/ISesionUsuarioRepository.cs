using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories;

public interface ISesionUsuarioRepository
{
    Task Crear(SesionUsuario sesion);
    Task<SesionUsuario?> ObtenerPorRefreshToken(string refreshToken);
    Task GuardarCambios();
}