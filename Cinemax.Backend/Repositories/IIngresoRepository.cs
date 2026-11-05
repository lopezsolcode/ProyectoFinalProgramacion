using Cinemax.Backend.Models.ModeloAuxiliar.Query.Ingreso;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface IIngresoRepository
    {
        Task<PaginadoResponse<Ingreso>> ObtenerTodos(IngresoQueryParametros parametros);
        Task<Ingreso?> ObtenerPorId(int id);
        Task<Ingreso?> ObtenerParaAnular(int id);
        Task Crear(Ingreso ingreso);
        Task GuardarCambios();
    }
}
