using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Sala;

namespace Cinemax.Backend.Repositories
{
    public interface ISalaRepository
    {
        Task<PaginadoResponse<SalaResumen>> ObtenerTodos(SalaQueryParametros parametros);
        Task<Sala?> ObtenerPorId(int id);
        Task<Sala?> ObtenerConButacas(int id);
        Task<int> ContarButacas(int id);
        Task<bool> ExistePorNumero(int numero, int? excluirId = null);
        Task<bool> TieneFuncionesFuturas(int id);
        Task Crear(Sala sala);
        Task Actualizar(Sala sala);
        Task Eliminar(Sala sala);
    }
}
