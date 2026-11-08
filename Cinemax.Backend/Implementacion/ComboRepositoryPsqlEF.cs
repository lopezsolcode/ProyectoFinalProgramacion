using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    // Implementacion/ComboRepositoryPsqlEF.cs
    public class ComboRepositoryPsqlEF : IComboRepository
    {
        private readonly DataContext _dataContext;
        public ComboRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<PaginadoResponse<Combo>> ObtenerTodos(ComboQueryParametros p)
        {
            IQueryable<Combo> query = _dataContext.Combos
                .Include(c => c.Imagen)
                .Include(c => c.Componentes).ThenInclude(x => x.Snack).ThenInclude(s => s.Inventario)
                .Where(c => !c.Eliminado)
                .AsNoTracking()
                .AsSplitQuery();

            if (!string.IsNullOrWhiteSpace(p.Buscar))
            {
                string buscar = p.Buscar.ToLower();
                query = query.Where(c => c.Nombre.ToLower().Contains(buscar));
            }

            bool desc = p.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = p.OrdenarPor?.ToLower() switch
            {
                "nombre" => desc ? query.OrderByDescending(c => c.Nombre).ThenBy(c => c.Id) : query.OrderBy(c => c.Nombre).ThenBy(c => c.Id),
                "precio" => desc ? query.OrderByDescending(c => c.Precio).ThenBy(c => c.Id) : query.OrderBy(c => c.Precio).ThenBy(c => c.Id),
                _ => desc ? query.OrderByDescending(c => c.Id) : query.OrderBy(c => c.Id)
            };

            int total = await query.CountAsync();
            List<Combo> datos = await query
                .Skip((p.NumeroPagina - 1) * p.TamanoPagina)
                .Take(p.TamanoPagina)
                .ToListAsync();

            return new PaginadoResponse<Combo>(datos, p.NumeroPagina, p.TamanoPagina, total);
        }
        public async Task<bool> Existe(int id) =>
            await _dataContext.Combos.AnyAsync(c => c.Id == id && !c.Eliminado);

        public async Task<Combo?> ObtenerPorId(int id)
        {
            return await _dataContext.Combos
                .Include(c => c.Imagen)
                .Include(c => c.Componentes).ThenInclude(x => x.Snack).ThenInclude(s => s.Inventario)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id && !c.Eliminado);
        }

        public async Task<List<Combo>> ObtenerPorIdsConComponentes(List<int> ids)
        {
            return await _dataContext.Combos
                .AsNoTracking()
                .Include(c => c.Componentes)
                .Where(c => ids.Contains(c.Id) && !c.Eliminado)
                .ToListAsync();
        }

        public async Task<bool> ExistePorNombre(string nombre, int? excluirId = null)
        {
            string n = nombre.ToLower();
            return await _dataContext.Combos.AnyAsync(c =>
                !c.Eliminado && c.Nombre.ToLower() == n && (excluirId == null || c.Id != excluirId));
        }

        public async Task Crear(Combo combo)
        {
            _dataContext.Combos.Add(combo);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Combo combo) => await _dataContext.SaveChangesAsync();

        public async Task Eliminar(Combo combo)
        {
            combo.Eliminado = true;
            await _dataContext.SaveChangesAsync();
        }
    }
}
