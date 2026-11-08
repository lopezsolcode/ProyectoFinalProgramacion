using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Funcion;

namespace Cinemax.Backend.Repositories
{
    // Repositories/IFuncionRepository.cs
    public interface IFuncionRepository
    {
        Task<PaginadoResponse<Funcion>> ObtenerTodos(FuncionQueryParametros parametros);
        Task<Funcion?> ObtenerPorId(int id);
        Task<List<Butaca>> ObtenerButacasDeSala(int salaId);
        Task<HashSet<int>> ObtenerButacasOcupadas(int funcionId);
        Task<bool> ExisteSolapamiento(int salaId, DateTime inicio, DateTime fin, int? excluirId = null);
        Task<bool> TieneReservasActivas(int funcionId);
        Task Crear(Funcion funcion);
        Task Actualizar(Funcion funcion);
        Task Eliminar(Funcion funcion);
    }
}
