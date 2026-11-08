using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Genero;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    // Implementacion/GeneroRepositoryPsqlEF.cs
    public class GeneroRepositoryPsqlEF : IGeneroRepository
    {
        private readonly DataContext _dataContext;
        public GeneroRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<PaginadoResponse<Genero>> ObtenerTodos(GeneroQueryParametros p)
        {
            IQueryable<Genero> query = _dataContext.Generos
                .Where(g => !g.Eliminado)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(p.Buscar))
            {
                string buscar = p.Buscar.ToLower();
                query = query.Where(g => g.Nombre.ToLower().Contains(buscar));
            }

            bool desc = p.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = p.OrdenarPor?.ToLower() switch
            {
                "nombre" => desc ? query.OrderByDescending(g => g.Nombre).ThenBy(g => g.Id) : query.OrderBy(g => g.Nombre).ThenBy(g => g.Id),
                _ => desc ? query.OrderByDescending(g => g.Id) : query.OrderBy(g => g.Id)
            };

            int total = await query.CountAsync();
            List<Genero> datos = await query
                .Skip((p.NumeroPagina - 1) * p.TamanoPagina)
                .Take(p.TamanoPagina)
                .ToListAsync();

            return new PaginadoResponse<Genero>(datos, p.NumeroPagina, p.TamanoPagina, total);
        }

        public async Task<Genero?> ObtenerPorId(int id)
        {
            return await _dataContext.Generos.FirstOrDefaultAsync(g => g.Id == id && !g.Eliminado);
        }

        public async Task<bool> ExistePorNombre(string nombre, int? excluirId = null)
        {
            string n = nombre.ToLower();
            return await _dataContext.Generos.AnyAsync(g =>
                !g.Eliminado && g.Nombre.ToLower() == n && (excluirId == null || g.Id != excluirId));
        }

        public async Task<bool> ExistenTodos(List<int> ids)
        {
            if (ids.Count == 0) return true;
            int encontrados = await _dataContext.Generos.CountAsync(g => ids.Contains(g.Id) && !g.Eliminado);
            return encontrados == ids.Count; // ids ya vienen sin duplicados
        }

        public async Task<bool> TienePeliculasAsociadas(int id)
        {
            return await _dataContext.GenerosPeliculas
                .AnyAsync(gp => gp.GeneroId == id && !gp.Pelicula.Eliminado);
        }

        public async Task Crear(Genero genero)
        {
            _dataContext.Generos.Add(genero);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Genero genero) => await _dataContext.SaveChangesAsync();

        public async Task Eliminar(Genero genero)
        {
            genero.Eliminado = true;
            await _dataContext.SaveChangesAsync();
        }
    }
}
