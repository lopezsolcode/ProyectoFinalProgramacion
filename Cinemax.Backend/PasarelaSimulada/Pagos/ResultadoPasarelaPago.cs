using Cinemax.Shared.Enums;

namespace Cinemax.Backend.PasarelaSimulada.Pagos
{
    public class ResultadoPasarelaPago
    {
        public required string ReferenciaPago { get; set; }

        public EstadoPago Estado { get; set; }

        public required string Mensaje { get; set; }
    }
}
