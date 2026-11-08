using Cinemax.Backend.Data;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    // Implementacion/InventarioRepositoryPsqlEF.cs
    public class InventarioRepositoryPsqlEF : IInventarioRepository
    {
        private readonly DataContext _dataContext;
        public InventarioRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<bool> TryAjustar(int snackId, int delta)
        {
            // Un solo UPDATE condicional: la verificación y el descuento son atómicos.
            // Corre dentro de la transacción abierta por el servicio que lo llama.
            // Ojo: ExecuteUpdate no pasa por el change tracker; no leas el inventario con
            // tracking antes y después en el mismo scope, porque verías el valor viejo.
            int filas = await _dataContext.Inventarios
                .Where(i => i.SnackId == snackId && i.CantidadDisponible + delta >= 0)
                .ExecuteUpdateAsync(s => s.SetProperty(i => i.CantidadDisponible, i => i.CantidadDisponible + delta));

            return filas == 1;
        }
    }
}
