using Microsoft.EntityFrameworkCore;
using Cinemax.Backend.Data;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Persona;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Models;


namespace Cinemax.Backend.Implementacion
{
    public class PersonaRepositoryPsqlEF : IPersonaRepository
    {
        private readonly DataContext _dataContext;
        public PersonaRepositoryPsqlEF(DataContext dataContext) => _dataContext = dataContext;

        public async Task<PaginadoResponse<Persona>> ObtenerTodos(PersonaQueryParametros p)
        {
            IQueryable<Persona> query = _dataContext.Personas.Include(a => a.Imagen).Where(a => !a.Eliminado)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(p.Buscar))
            {
                string buscar = p.Buscar.ToLower();
                query = query.Where(a => a.Nombre.ToLower().Contains(buscar));
            }

            bool desc = p.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) == true;
            query = p.OrdenarPor?.ToLower() switch
            {
                "nombre" => desc ? query.OrderByDescending(a => a.Nombre).ThenBy(a => a.Id) : query.OrderBy(a => a.Nombre).ThenBy(a => a.Id),
                _ => desc ? query.OrderByDescending(a => a.Id) : query.OrderBy(a => a.Id)
            };

            int total = await query.CountAsync();
            List<Persona> datos = await query
                .Skip((p.NumeroPagina - 1) * p.TamanoPagina)
                .Take(p.TamanoPagina)
                .ToListAsync();

            return new PaginadoResponse<Persona>(datos, p.NumeroPagina, p.TamanoPagina, total);
        }

        public async Task<Persona?> ObtenerPorId(int id)
        {
            return await _dataContext.Personas.Include(a => a.Imagen).FirstOrDefaultAsync(a => a.Id == id && !a.Eliminado);
        }

        public async Task<bool> ExistenTodos(List<int> ids)
        {
            if (ids.Count == 0) return true;
            int encontrados = await _dataContext.Personas.CountAsync(a => ids.Contains(a.Id) && !a.Eliminado);
            return encontrados == ids.Count; // ids ya vienen sin duplicados
        }

        public async Task<bool> TienePeliculasAsociadas(int id)
        {
            return await _dataContext.PersonasPeliculas
                .AnyAsync(pp => pp.PersonaId == id && !pp.Pelicula.Eliminado);
        }

        public async Task Crear(Persona Persona)
        {
            _dataContext.Personas.Add(Persona);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Persona Persona) => await _dataContext.SaveChangesAsync();

        public async Task Eliminar(Persona Persona)
        {
            Persona.Eliminado = true;
            await _dataContext.SaveChangesAsync();
        }
    }
}
