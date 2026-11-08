using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.PasarelaSimulada.Pagos;
using Cinemax.Backend.Repositories;
using Cinemax.Shared.Enums;
namespace Cinemax.Backend.Services
{
    public class PagoService
    {
        private readonly IVentaRepository _ventaRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IPasarelaPago _pasarela;
        private readonly VentaService _ventaService;
        private readonly IUnidadDeTrabajo _uow;
        private readonly ILogger<PagoService> _logger;

        public PagoService(IVentaRepository ventaRepo, IClienteRepository clienteRepo, IPasarelaPago pasarela,
            VentaService ventaService, IUnidadDeTrabajo uow, ILogger<PagoService> logger)
        {
            _ventaRepo = ventaRepo; _clienteRepo = clienteRepo; _pasarela = pasarela;
            _ventaService = ventaService; _uow = uow; _logger = logger;
        }

        public async Task<ResultadoPagoAplicado> ProcesarPago(int ventaId, int usuarioId, MetodoPago metodo)
        {
            // Si tu enum tiene Efectivo, rechazalo acá: el pago online es solo por la pasarela.
            if (!Enum.IsDefined(metodo))
                throw new DatosLlegaronErradosException("El método de pago no es válido.");

            Cliente cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId)
                ?? throw new RecursoNoExisteException("No existe ninguna compra con ese id.");

            // Si ya venció, se libera ahora y el pago no procede.
            await _ventaService.VencerReserva(ventaId);

            // 1) Registrar el intento de pago (transacción corta).
            (int pagoId, decimal monto) = await _uow.Ejecutar(async () =>
            {
                Venta? venta = await _ventaRepo.ObtenerPorId(ventaId);
                if (venta is null || venta.ClienteId != cliente.Id)
                    throw new RecursoNoExisteException("No existe ninguna compra con ese id.");
                if (venta.Estado != EstadoVenta.Pendiente)
                    throw new DatosLlegaronErradosException("La compra ya no está pendiente de pago (venció o fue cancelada).");

                int id = await _ventaRepo.CrearPagoPendiente(ventaId, metodo, venta.Total); // monto del servidor
                return (id, venta.Total);
            });

            // 2) Llamar a la pasarela FUERA de la transacción: no se retienen bloqueos mientras se espera.
            ResultadoPasarelaPago resultado;
            try
            {
                resultado = await _pasarela.ProcesarPagoAsync(ventaId, monto, metodo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falló la pasarela de pago para la venta {VentaId}", ventaId);
                resultado = new ResultadoPasarelaPago
                {
                    ReferenciaPago = $"ERROR-{Guid.NewGuid():N}",
                    Estado = EstadoPago.Rechazado,
                    Mensaje = "La pasarela de pago no respondió."
                };
            }

            // 3) Aplicar el resultado (transacción corta).
            return await _ventaService.AplicarResultadoPago(ventaId, pagoId, resultado, usuarioId);
        }
    }
}
