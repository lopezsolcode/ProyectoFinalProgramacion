using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack;

namespace Cinemax.Backend.Repositories
{
    public interface ISnackRepository
    {
        Task<PaginadoResponse<Snack>> ObtenerTodos(SnackQueryParametros parametros);
        Task<Snack?> ObtenerPorId(int id);
        Task<List<Snack>> ObtenerPorIds(List<int> ids);
        Task<bool> Existe(int id);
        Task<bool> ExistePorNombre(string nombre, int? excluirId = null);
        Task<bool> ExistenTodos(List<int> ids);
        Task<bool> EstaEnCombosActivos(int id);
        Task Crear(Snack snack);
        Task Actualizar(Snack snack);
        Task Eliminar(Snack snack);
    }

    public interface IInventarioRepository
    {
        // delta negativo descuenta, positivo devuelve. Devuelve false si dejaría el stock en negativo
        // o si no existe el inventario.
        Task<bool> TryAjustar(int snackId, int delta);
    }

    public interface IComboRepository
    {
        Task<PaginadoResponse<Combo>> ObtenerTodos(ComboQueryParametros parametros);
        Task<Combo?> ObtenerPorId(int id);
        Task<List<Combo>> ObtenerPorIdsConComponentes(List<int> ids);
        Task<bool> ExistePorNombre(string nombre, int? excluirId = null);
        Task Crear(Combo combo);
        Task<bool> Existe(int id);
        Task Actualizar(Combo combo);
        Task Eliminar(Combo combo);
    }
}
