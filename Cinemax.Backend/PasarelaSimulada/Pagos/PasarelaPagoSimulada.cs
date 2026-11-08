using Cinemax.Backend.Options;
using Cinemax.Shared.Enums;
using Microsoft.Extensions.Options;
namespace Cinemax.Backend.PasarelaSimulada.Pagos
{
    public class PasarelaPagoSimulada : IPasarelaPago
    {
        private readonly PasarelaSimuladaOptions _opciones;
        public PasarelaPagoSimulada(IOptions<PasarelaSimuladaOptions> opciones) => _opciones = opciones.Value;

        public Task<ResultadoPasarelaPago> ProcesarPagoAsync(int ventaId, decimal monto, MetodoPago metodo)
        {
            bool aprobar = _opciones.Modo switch
            {
                ModoPasarelaSimulada.Rechazar => false,
                ModoPasarelaSimulada.SegunCentavos => decimal.Truncate((monto - decimal.Truncate(monto)) * 100) != 99,
                _ => true
            };

            return Task.FromResult(new ResultadoPasarelaPago
            {
                ReferenciaPago = $"SIM-{Guid.NewGuid():N}",
                Estado = aprobar ? EstadoPago.Aprobado : EstadoPago.Rechazado,
                Mensaje = aprobar ? "Pago aprobado por la pasarela simulada." : "Pago rechazado por la pasarela simulada."
            });
        }
    }
}
