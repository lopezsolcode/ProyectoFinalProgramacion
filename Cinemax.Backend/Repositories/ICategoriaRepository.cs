using Cinemax.Backend.Models.ModeloAuxiliar.Query.Categoria;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface ICategoriaRepository
    {
        Task<PaginadoResponse<Categoria>> ObtenerTodas(CategoriaQueryParametros parametros);
        Task<Categoria?> ObtenerPorId(int id);
        Task<bool> ExistePorNombre(string nombre);
        Task<bool> TieneProductosAsociados(int categoriaId);

        Task Crear(Categoria categoria);
        Task Actualizar(Categoria categoria);
        Task Eliminar(Categoria categoria);
    }
}
