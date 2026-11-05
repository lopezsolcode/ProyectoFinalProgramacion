using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories
{
    public interface IPagoRepository
    {
        Task Crear(Pago pago);
        Task GuardarCambios();
    }
}
