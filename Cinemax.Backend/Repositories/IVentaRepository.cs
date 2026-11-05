using Cinemax.Backend.Models.ModeloAuxiliar.Query.Venta;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface IVentaRepository
    {
        Task<PaginadoResponse<Venta>> ObtenerTodas(VentaQueryParametros parametros);
        Task<Venta?> ObtenerPorId(int id);
        Task<Venta?> ObtenerParaAnular(int id);
        Task<Venta?> ObtenerParaProcesarPago(int id);
        Task Crear(Venta venta);
        Task GuardarCambios();
        Task<Venta?> ObtenerParaTicket(int id);
    }
}
