using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface IRegisterRepository
    {
        Task Registrarse(Usuario usuario);
    }
}
