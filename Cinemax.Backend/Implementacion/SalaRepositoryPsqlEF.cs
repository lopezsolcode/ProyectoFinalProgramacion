using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Sala;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    // Implementacion/SalaRepositoryPsqlEF.cs
    public class SalaRepositoryPsqlEF : ISalaRepository
    {
        private readonly DataContext _dataContext;
        public SalaRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<PaginadoResponse<SalaResumen>> ObtenerTodos(SalaQueryParametros p)
        {
            IQueryable<Sala> query = _dataContext.Salas.Where(s => !s.Eliminado).AsNoTracking();

            bool desc = p.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = p.OrdenarPor?.ToLower() switch
            {
                "numero" => desc ? query.OrderByDescending(s => s.Numero).ThenBy(s => s.Id) : query.OrderBy(s => s.Numero).ThenBy(s => s.Id),
                _ => desc ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
            };

            int total = await query.CountAsync();
            List<SalaResumen> datos = await query
                .Skip((p.NumeroPagina - 1) * p.TamanoPagina)
                .Take(p.TamanoPagina)
                .Select(s => new SalaResumen(s.Id, s.Numero, s.Butacas.Count))
                .ToListAsync();

            return new PaginadoResponse<SalaResumen>(datos, p.NumeroPagina, p.TamanoPagina, total);
        }

        public async Task<Sala?> ObtenerPorId(int id)
        {
            return await _dataContext.Salas.FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado);
        }

        public async Task<Sala?> ObtenerConButacas(int id)
        {
            return await _dataContext.Salas
                .AsNoTracking()
                .Include(s => s.Butacas.OrderBy(b => b.Fila).ThenBy(b => b.Columna))
                .FirstOrDefaultAsync(s => s.Id == id && !s.Eliminado);
        }

        public async Task<int> ContarButacas(int id)
        {
            return await _dataContext.Butacas.CountAsync(b => b.SalaId == id);
        }

        public async Task<bool> ExistePorNumero(int numero, int? excluirId = null)
        {
            return await _dataContext.Salas.AnyAsync(s =>
                !s.Eliminado && s.Numero == numero && (excluirId == null || s.Id != excluirId));
        }

        public async Task<bool> TieneFuncionesFuturas(int id)
        {
            DateTime ahora = DateTime.UtcNow;
            return await _dataContext.Funciones.AnyAsync(f =>
                f.SalaId == id && !f.Eliminado && f.FechaHora > ahora);
        }
        public async Task Crear(Sala sala)
        {
            _dataContext.Salas.Add(sala); // las butacas viajan en el grafo: un solo SaveChanges
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Sala sala) => await _dataContext.SaveChangesAsync();

        public async Task Eliminar(Sala sala)
        {
            sala.Eliminado = true;
            await _dataContext.SaveChangesAsync();
        }
 
    }
}
