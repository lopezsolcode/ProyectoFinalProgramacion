using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Genero;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.DTO.Genero.Request;
using Cinemax.Shared.DTO.Genero.Response;
using Cinemax.Shared.DTO.Paginado.Request.Genero;

namespace Cinemax.Backend.Services
{
    public class GeneroService
    {
        private readonly IGeneroRepository _repo;
        public GeneroService(IGeneroRepository repo) => _repo = repo;

        public async Task<PaginadoResponse<GeneroResponse>> ObtenerTodos(ParametroPaginacionGeneroRequest p)
        {
            var query = new GeneroQueryParametros
            {
                NumeroPagina = Math.Max(p.NumeroPagina ?? 1, 1),
                TamanoPagina = Math.Clamp(p.TamanoPagina ?? 20, 1, 50),
                Buscar = p.Buscar?.Trim().ToLower(),
                OrdenarPor = p.OrdenarPor,
                Direccion = p.Direccion
            };

            var r = await _repo.ObtenerTodos(query);
            var datos = r.Datos.Select(g => new GeneroResponse(g.Id, g.Nombre)).ToList();
            return new PaginadoResponse<GeneroResponse>(datos, r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<GeneroResponse> ObtenerPorId(int id)
        {
            Genero genero = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún género con ese id.");
            return new GeneroResponse(genero.Id, genero.Nombre);
        }

        public async Task<GeneroResponse> Crear(CrearGeneroRequest dto)
        {
            ValidarNombre(dto.Nombre);
            string nombre = dto.Nombre.Trim();

            if (await _repo.ExistePorNombre(nombre))
                throw new RecursoExistenteException("Ya existe un género con ese nombre.");

            Genero genero = new() { Nombre = nombre };
            await _repo.Crear(genero);
            return new GeneroResponse(genero.Id, genero.Nombre);
        }

        public async Task Actualizar(int id, ActualizarGeneroRequest dto)
        {
            ValidarNombre(dto.Nombre);
            string nombre = dto.Nombre.Trim();

            Genero genero = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún género con ese id.");

            if (await _repo.ExistePorNombre(nombre, excluirId: id))
                throw new RecursoExistenteException("Ya existe un género con ese nombre.");

            genero.Nombre = nombre;
            await _repo.Actualizar(genero);
        }

        public async Task Eliminar(int id)
        {
            Genero genero = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún género con ese id.");

            if (await _repo.TienePeliculasAsociadas(id))
                throw new RecursoExistenteException("No se puede eliminar: hay películas con este género.");

            await _repo.Eliminar(genero);
        }

        private static void ValidarNombre(string nombre)
        {
            if (!Validaciones.EstanDatosBien(nombre))
                throw new DatosLlegaronErradosException("El nombre del género es obligatorio.");
            if (nombre.Trim().Length > 100)
                throw new DatosLlegaronErradosException("El nombre no puede superar 100 caracteres.");
        }
    }
}
