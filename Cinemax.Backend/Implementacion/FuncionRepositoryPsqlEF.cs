using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Funcion;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    // Implementacion/FuncionRepositoryPsqlEF.cs
    public class FuncionRepositoryPsqlEF : IFuncionRepository
    {
        private readonly DataContext _dataContext;
        public FuncionRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<PaginadoResponse<Funcion>> ObtenerTodos(FuncionQueryParametros p)
        {
            IQueryable<Funcion> query = _dataContext.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                .Where(f => !f.Eliminado)
                .AsNoTracking();

            if (p.SoloFuturas)
            {
                DateTime ahora = DateTime.UtcNow;
                query = query.Where(f => f.FechaHora > ahora && !f.Pelicula.Eliminado && !f.Sala.Eliminado);
            }
            if (p.PeliculaId.HasValue) query = query.Where(f => f.PeliculaId == p.PeliculaId.Value);
            if (p.SalaId.HasValue) query = query.Where(f => f.SalaId == p.SalaId.Value);
            if (p.Desde.HasValue) query = query.Where(f => f.FechaHora >= p.Desde.Value);
            if (p.Hasta.HasValue) query = query.Where(f => f.FechaHora < p.Hasta.Value);

            bool desc = p.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = p.OrdenarPor?.ToLower() switch
            {
                "id" => desc ? query.OrderByDescending(f => f.Id) : query.OrderBy(f => f.Id),
                _ => desc ? query.OrderByDescending(f => f.FechaHora).ThenBy(f => f.Id)
                             : query.OrderBy(f => f.FechaHora).ThenBy(f => f.Id)
            };

            int total = await query.CountAsync();
            List<Funcion> datos = await query
                .Skip((p.NumeroPagina - 1) * p.TamanoPagina)
                .Take(p.TamanoPagina)
                .ToListAsync();

            return new PaginadoResponse<Funcion>(datos, p.NumeroPagina, p.TamanoPagina, total);
        }

        public async Task<Funcion?> ObtenerPorId(int id)
        {
            return await _dataContext.Funciones
                .Include(f => f.Pelicula)
                .Include(f => f.Sala)
                .FirstOrDefaultAsync(f => f.Id == id && !f.Eliminado);
        }

        public async Task<List<Butaca>> ObtenerButacasDeSala(int salaId)
        {
            return await _dataContext.Butacas
                .AsNoTracking()
                .Where(b => b.SalaId == salaId)
                .OrderBy(b => b.Fila).ThenBy(b => b.Columna)
                .ToListAsync();
        }

        public async Task<HashSet<int>> ObtenerButacasOcupadas(int funcionId)
        {
            List<int> ids = await _dataContext.EntradasVenta
                .Where(e => e.FuncionId == funcionId && e.Activa)
                .Select(e => e.ButacaId)
                .ToListAsync();
            return ids.ToHashSet();
        }

        public async Task<bool> ExisteSolapamiento(int salaId, DateTime inicio, DateTime fin, int? excluirId = null)
        {
            return await _dataContext.Funciones.AnyAsync(f =>
                !f.Eliminado && f.SalaId == salaId &&
                f.FechaHora < fin && f.FechaFin > inicio &&
                (excluirId == null || f.Id != excluirId));
        }

        public async Task<bool> TieneReservasActivas(int funcionId)
        {
            return await _dataContext.EntradasVenta.AnyAsync(e => e.FuncionId == funcionId && e.Activa);
        }

        public async Task Crear(Funcion funcion)
        {
            _dataContext.Funciones.Add(funcion);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Funcion funcion) => await _dataContext.SaveChangesAsync();

        public async Task Eliminar(Funcion funcion)
        {
            funcion.Eliminado = true;
            await _dataContext.SaveChangesAsync();
        }
    }
}
