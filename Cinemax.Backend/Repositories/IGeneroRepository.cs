using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Genero;

namespace Cinemax.Backend.Repositories
{
    public interface IGeneroRepository
    {
        Task<PaginadoResponse<Genero>> ObtenerTodos(GeneroQueryParametros parametros);
        Task<Genero?> ObtenerPorId(int id);
        Task<bool> ExistePorNombre(string nombre, int? excluirId = null);
        Task<bool> ExistenTodos(List<int> ids);
        Task<bool> TienePeliculasAsociadas(int id);
        Task Crear(Genero genero);
        Task Actualizar(Genero genero);
        Task Eliminar(Genero genero);
    }
}
