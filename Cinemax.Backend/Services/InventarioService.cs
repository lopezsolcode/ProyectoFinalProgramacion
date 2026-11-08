using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack;
using Cinemax.Backend.Repositories;

namespace Cinemax.Backend.Services
{
    // Services/InventarioService.cs: lo usa la venta en el paso 4
    public class InventarioService
    {
        private readonly IInventarioRepository _repo;
        private readonly ISnackRepository _snackRepo;
        private readonly IComboRepository _comboRepo;

        public InventarioService(IInventarioRepository repo, ISnackRepository snackRepo, IComboRepository comboRepo)
        {
            _repo = repo; _snackRepo = snackRepo; _comboRepo = comboRepo;
        }

        // Traduce lo que se quiere comprar (snacks y combos) a unidades de cada snack.
        public async Task<Dictionary<int, int>> CalcularConsumo(IEnumerable<ItemSnackVenta> items)
        {
            List<ItemSnackVenta> lista = items.ToList();

            if (lista.Any(i => (i.SnackId is null) == (i.ComboId is null)))
                throw new DatosLlegaronErradosException("Cada ítem debe ser un snack o un combo, no ambos.");
            if (lista.Any(i => i.Cantidad < 1 || i.Cantidad > 50))
                throw new DatosLlegaronErradosException("La cantidad de cada ítem debe estar entre 1 y 50.");

            List<int> comboIds = lista.Where(i => i.ComboId.HasValue).Select(i => i.ComboId!.Value).Distinct().ToList();
            Dictionary<int, Combo> combos = (await _comboRepo.ObtenerPorIdsConComponentes(comboIds))
                .ToDictionary(c => c.Id);

            Dictionary<int, int> consumo = new();

            foreach (ItemSnackVenta item in lista)
            {
                if (item.SnackId.HasValue)
                {
                    consumo[item.SnackId.Value] = consumo.GetValueOrDefault(item.SnackId.Value) + item.Cantidad;
                    continue;
                }

                if (!combos.TryGetValue(item.ComboId!.Value, out Combo? combo))
                    throw new RecursoNoExisteException("Algún combo indicado no existe.");

                foreach (ComboSnack comp in combo.Componentes)
                    consumo[comp.SnackId] = consumo.GetValueOrDefault(comp.SnackId) + comp.Cantidad * item.Cantidad;
            }

            return consumo;
        }

        // Debe ejecutarse dentro de una transacción: si falta stock de uno, se revierten los demás.
        // Se recorre ordenado por SnackId para que dos ventas simultáneas tomen los bloqueos
        // en el mismo orden y no se produzca un deadlock.
        public async Task Descontar(Dictionary<int, int> consumo)
        {
            foreach (var (snackId, cantidad) in consumo.OrderBy(x => x.Key))
            {
                if (await _repo.TryAjustar(snackId, -cantidad)) continue;

                string nombre = (await _snackRepo.ObtenerPorIds(new List<int> { snackId }))
                    .FirstOrDefault()?.Nombre ?? "un producto";
                throw new StockInsuficienteException($"No hay stock suficiente de {nombre}.");
            }
        }

        public async Task Devolver(Dictionary<int, int> consumo)
        {
            foreach (var (snackId, cantidad) in consumo.OrderBy(x => x.Key))
            {
                if (!await _repo.TryAjustar(snackId, cantidad))
                    throw new InvalidOperationException($"No se pudo devolver stock del snack {snackId}.");
            }
        }
    }
}
