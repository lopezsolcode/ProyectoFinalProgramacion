namespace Cinemax.Backend.Services
{
    // Libera cada 30 segundos las compras pendientes vencidas (butacas y stock reservados).
    // Sin esto, una reserva abandonada seguía ocupando butacas y stock hasta que alguien creara otra venta.
    public class ReservasVencidasWorker : BackgroundService
    {
        private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(30);

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<ReservasVencidasWorker> _logger;

        public ReservasVencidasWorker(IServiceScopeFactory scopeFactory, ILogger<ReservasVencidasWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using PeriodicTimer temporizador = new(Intervalo);

            do
            {
                try
                {
                    using IServiceScope scope = _scopeFactory.CreateScope();
                    VentaService ventas = scope.ServiceProvider.GetRequiredService<VentaService>();
                    await ventas.LiberarVencidas();
                }
                catch (Exception e) when (e is not OperationCanceledException)
                {
                    _logger.LogError(e, "No se pudieron liberar las reservas vencidas.");
                }
            }
            while (await temporizador.WaitForNextTickAsync(stoppingToken));
        }
    }
}
