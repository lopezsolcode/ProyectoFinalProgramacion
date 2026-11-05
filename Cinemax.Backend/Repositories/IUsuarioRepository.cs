using Cinemax.Backend.Models.ModeloAuxiliar.Query.Usuario;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface IUsuarioRepository
    {
        Task<bool> ExistePorUsername(string username);
        Task<bool> ExistePorEmail(string email);
        Task<Usuario?> BuscarPorUsername(string username);
        Task<Usuario?> BuscarPorId(int id);
        Task DarDeBaja(Usuario usuario);
        Task<PaginadoResponse<Usuario>> ObtenerTodos(UsuarioQueryParametros usuarioQueryParametros);
        Task Actualizar(Usuario usuario);
        Task<int>ContarAdministradores();
    }
}
