using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    public class PeliculaRepositoryPsqlEF : IPeliculaRepository
    {
        private readonly DataContext _dataContext;
        public PeliculaRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<PaginadoResponse<Pelicula>> ObtenerTodos(PeliculaQueryParametros p)
        {
            IQueryable<Pelicula> query = _dataContext.Peliculas
                .Include(x => x.Generos).ThenInclude(g => g.Genero)
                .Where(x => !x.Eliminado)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(p.Buscar))
            {
                string buscar = p.Buscar.ToLower();
                query = query.Where(x => x.Titulo.ToLower().Contains(buscar));
            }
            if (p.GeneroId.HasValue)
                query = query.Where(x => x.Generos.Any(g => g.GeneroId == p.GeneroId.Value));
            if (p.EnCartelera.HasValue)
                query = query.Where(x => x.EnCartelera == p.EnCartelera.Value);

            bool desc = p.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = p.OrdenarPor?.ToLower() switch
            {
                "titulo" => desc ? query.OrderByDescending(x => x.Titulo).ThenBy(x => x.Id) : query.OrderBy(x => x.Titulo).ThenBy(x => x.Id),
                "estreno" => desc ? query.OrderByDescending(x => x.FechaEstreno).ThenBy(x => x.Id) : query.OrderBy(x => x.FechaEstreno).ThenBy(x => x.Id),
                "duracion" => desc ? query.OrderByDescending(x => x.DuracionMinutos).ThenBy(x => x.Id) : query.OrderBy(x => x.DuracionMinutos).ThenBy(x => x.Id),
                _ => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id)
            };

            int total = await query.CountAsync();
            List<Pelicula> datos = await query
                .Skip((p.NumeroPagina - 1) * p.TamanoPagina)
                .Take(p.TamanoPagina)
                .ToListAsync();

            return new PaginadoResponse<Pelicula>(datos, p.NumeroPagina, p.TamanoPagina, total);
        }

        public async Task<Pelicula?> ObtenerPorId(int id)
        {
            // Con tracking a propósito: Actualizar sincroniza las colecciones sobre esta instancia.
            return await _dataContext.Peliculas
                .Include(x => x.Generos).ThenInclude(g => g.Genero)
                .Include(x => x.Personas).ThenInclude(p => p.Persona)
                .AsSplitQuery()
                .FirstOrDefaultAsync(x => x.Id == id && !x.Eliminado);
        }

        public async Task<bool> ExistePorTitulo(string titulo, DateOnly fechaEstreno, int? excluirId = null)
        {
            return await _dataContext.Peliculas.AnyAsync(x =>
                !x.Eliminado && x.Titulo == titulo && x.FechaEstreno == fechaEstreno &&
                (excluirId == null || x.Id != excluirId));
        }

        public async Task Crear(Pelicula pelicula)
        {
            _dataContext.Peliculas.Add(pelicula);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Pelicula pelicula)
        {
            await _dataContext.SaveChangesAsync(); // ya está trackeada
        }

        public async Task Eliminar(Pelicula pelicula)
        {
            pelicula.Eliminado = true;
            await _dataContext.SaveChangesAsync();
        }
    }
}
