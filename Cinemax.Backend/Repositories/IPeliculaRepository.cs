using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query;

namespace Cinemax.Backend.Repositories
{
    public interface IPeliculaRepository
    {
        Task<PaginadoResponse<Pelicula>> ObtenerTodos(PeliculaQueryParametros parametros);
        Task<Pelicula?> ObtenerPorId(int id);
        Task<bool> ExistePorTitulo(string titulo, DateOnly fechaEstreno, int? excluirId = null);
        Task Crear(Pelicula pelicula);
        Task Actualizar(Pelicula pelicula);
        Task Eliminar(Pelicula pelicula);
        Task<int?> ObtenerDuracionMinutos(int id);
        Task<bool> TieneFuncionesFuturas(int id);
    }
}
