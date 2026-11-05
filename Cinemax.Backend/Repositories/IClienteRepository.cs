using Cinemax.Backend.Models.ModeloAuxiliar.Query.Cliente;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface IClienteRepository
    {
        Task<PaginadoResponse<Cliente>> ObtenerTodos(ClienteQueryParametros parametros);
        Task<Cliente?> ObtenerPorUsuarioId(int usuarioId);
        Task<Cliente?> ObtenerPorId(int id);
        Task<bool> ExistePorDni(string dni);
        Task<bool> ExistePorEmail(string email);
        Task<bool> TieneVentasAsociadas(int clienteId);

        Task Crear(Cliente cliente);
        Task Actualizar(Cliente cliente);
        Task Eliminar(Cliente cliente);
    }
}
