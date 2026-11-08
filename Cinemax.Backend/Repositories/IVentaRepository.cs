using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Venta;
using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Repositories
{
    public interface IVentaRepository
    {
        Task<PaginadoResponse<Venta>> ObtenerTodas(VentaQueryParametros parametros);
        Task<Venta?> ObtenerPorId(int id);             // AsNoTracking, con todo el detalle
        Task<Venta?> ObtenerParaTicket(int id);        // igual que ObtenerPorId
        Task Crear(Venta venta);
        Task<bool> TryCambiarEstado(int ventaId, EstadoVenta desde, EstadoVenta hasta, bool soloSiVencida = false);
        Task ActualizarEstadoPagos(int ventaId, EstadoPago desde, EstadoPago hasta);
        Task ResolverPago(int pagoId, EstadoPago estado, string? referencia);
        Task<int> CrearPagoPendiente(int ventaId, MetodoPago metodo, decimal monto);
        Task DesactivarEntradas(int ventaId);
        Task<Dictionary<int, int>> ObtenerConsumoStock(int ventaId);
        Task<List<int>> ObtenerIdsVencidas(DateTime ahora);
        Task<bool> TienePendiente(int clienteId, DateTime ahora);
        // FALTA OBTENER PARA PROCESAR PAGO

    }
}
