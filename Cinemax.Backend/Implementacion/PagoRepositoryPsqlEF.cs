using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Repositories;

namespace Cinemax.Backend.Implementacion
{
    public class PagoRepositoryPsqlEF : IPagoRepository
    {
        private readonly DataContext _dataContext;

        public PagoRepositoryPsqlEF(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task Crear(Pago pago)
        {
            _dataContext.Pagos.Add(pago);
            await _dataContext.SaveChangesAsync();
        }

        public async Task GuardarCambios()
        {
            await _dataContext.SaveChangesAsync();
        }
    }
}

