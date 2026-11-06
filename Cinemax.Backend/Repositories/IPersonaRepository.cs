using Cinemax.Backend.Models.Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Persona;

namespace Cinemax.Backend.Repositories
{
    public interface IPersonaRepository
    {
        Task<PaginadoResponse<Persona>> ObtenerTodos(PersonaQueryParametros parametros);
        Task<Persona?> ObtenerPorId(int id);
        Task<bool> ExistenTodos(List<int> ids);
        Task<bool> TienePeliculasAsociadas(int id);
        Task Crear(Persona persona);
        Task Actualizar(Persona persona);
        Task Eliminar(Persona persona);
    }
}
