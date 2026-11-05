using Cinemax.Backend.Models.ModeloAuxiliar.Query.Proveedor;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories;

public interface IProveedorRepository
{
    Task<PaginadoResponse<Proveedor>> ObtenerTodos(ProveedorQueryParametros proveedorQueryParametros);
    Task<Proveedor?> BuscarPorId(int id);
    Task<bool> ExistePorCuit(string cuit);
    Task<bool> ExistePorEmail(string email);
    Task<bool> ExistePorRazonSocial(string razonSocial);
    Task<bool> ExistePorTelefono(string telefono);
    Task<bool> TieneIngresosAsociados(int proveedorId);
    Task Crear(Proveedor proveedor);
    Task Actualizar(Proveedor proveedor);
    Task DarDeBaja(Proveedor proveedor);
}