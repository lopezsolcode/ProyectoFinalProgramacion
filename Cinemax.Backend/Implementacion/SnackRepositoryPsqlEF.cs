using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    public class SnackRepositoryPsqlEF : ISnackRepository
    {
        private readonly DataContext _dataContext;
        public SnackRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<PaginadoResponse<Snack>> ObtenerTodos(SnackQueryParametros p)
        {
            IQueryable<Snack> query = _dataContext.Snacks
                .Include(s => s.Inventario)
                .Include(s => s.Imagen)
                .Where(s => !s.Eliminado)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(p.Buscar))
            {
                string buscar = p.Buscar.ToLower();
                query = query.Where(s => s.Nombre.ToLower().Contains(buscar));
            }
            if (p.SoloDisponibles)
                query = query.Where(s => s.Inventario.CantidadDisponible > 0);

            bool desc = p.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = p.OrdenarPor?.ToLower() switch
            {
                "nombre" => desc ? query.OrderByDescending(s => s.Nombre).ThenBy(s => s.Id) : query.OrderBy(s => s.Nombre).ThenBy(s => s.Id),
                "precio" => desc ? query.OrderByDescending(s => s.Precio).ThenBy(s => s.Id) : query.OrderBy(s => s.Precio).ThenBy(s => s.Id),
                "stock" => desc ? query.OrderByDescending(s => s.Inventario.CantidadDisponible).ThenBy(s => s.Id)
                                 : query.OrderBy(s => s.Inventario.CantidadDisponible).ThenBy(s => s.Id),
                _ => desc ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
            };

            int total = await query.CountAsync();
            List<Snack> datos = await query
                .Skip((p.NumeroPagina - 1) * p.TamanoPagina)
                .Take(p.TamanoPagina)
                .ToListAsync();

            return new PaginadoResponse<Snack>(datos, p.NumeroPagina, p.TamanoPagina, total);
        }

        public async Task<Snack?> ObtenerPorId(int id)
        {
            return await _dataContext.Snacks
                .Include(s => s.Inventario)
                .Include(s => s.Imagen)
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado);
        }

        public async Task<List<Snack>> ObtenerPorIds(List<int> ids)
        {
            return await _dataContext.Snacks
                .AsNoTracking()
                .Where(s => ids.Contains(s.Id) && !s.Eliminado)
                .ToListAsync();
        }

        public async Task<bool> Existe(int id)
        {
            return await _dataContext.Snacks.AnyAsync(s => s.Id == id && !s.Eliminado);
        }

        public async Task<bool> ExistePorNombre(string nombre, int? excluirId = null)
        {
            string n = nombre.ToLower();
            return await _dataContext.Snacks.AnyAsync(s =>
                !s.Eliminado && s.Nombre.ToLower() == n && (excluirId == null || s.Id != excluirId));
        }

        public async Task<bool> ExistenTodos(List<int> ids)
        {
            if (ids.Count == 0) return true;
            int encontrados = await _dataContext.Snacks.CountAsync(s => ids.Contains(s.Id) && !s.Eliminado);
            return encontrados == ids.Count; // ids ya vienen sin duplicados
        }

        public async Task<bool> EstaEnCombosActivos(int id)
        {
            return await _dataContext.ComboSnacks.AnyAsync(cs => cs.SnackId == id && !cs.Combo.Eliminado);
        }

        public async Task Crear(Snack snack)
        {
            _dataContext.Snacks.Add(snack); // el Inventario viaja en el grafo
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Snack snack) => await _dataContext.SaveChangesAsync();

        public async Task Eliminar(Snack snack)
        {
            snack.Eliminado = true;
            await _dataContext.SaveChangesAsync();
        }
    }
}
