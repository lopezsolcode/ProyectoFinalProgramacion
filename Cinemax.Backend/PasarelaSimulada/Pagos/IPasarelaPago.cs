using Cinemax.Backend.Models;
using Cinemax.Shared.Enums;
namespace Cinemax.Backend.PasarelaSimulada.Pagos
{
    public interface IPasarelaPago
    {
        Task<ResultadoPasarelaPago> ProcesarPagoAsync(
            int ventaId,
            decimal monto,
            MetodoPago metodo);
    }
}
