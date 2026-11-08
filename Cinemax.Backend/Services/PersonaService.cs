using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Persona;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.DTO.Paginado.Request.Persona;
using Cinemax.Shared.DTO.Persona.Request;
using Cinemax.Shared.DTO.Persona.Response;

namespace Cinemax.Backend.Services
{
    // Services/ActorService.cs
    public class PersonaService
    {
        private readonly IPersonaRepository _repo;

        private readonly ImagenMapeador _imagenes;
        public PersonaService(IPersonaRepository repo, ImagenMapeador imagenes)
        {
            _repo = repo; _imagenes = imagenes;
        }

        public async Task<PaginadoResponse<PersonaResponse>> ObtenerTodos(ParametroPaginacionPersonaRequest p)
        {
            var query = new PersonaQueryParametros
            {
                NumeroPagina = Math.Max(p.NumeroPagina ?? 1, 1),
                TamanoPagina = Math.Clamp(p.TamanoPagina ?? 20, 1, 50),
                Buscar = p.Buscar?.Trim().ToLower(),
                OrdenarPor = p.OrdenarPor,
                Direccion = p.Direccion
            };

            var r = await _repo.ObtenerTodos(query);
            var datos = r.Datos.Select(Mapear).ToList();
            return new PaginadoResponse<PersonaResponse>(datos, r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }
        private PersonaResponse Mapear(Persona p) =>
            new(p.Id, p.Nombre, _imagenes.Mapear(p.Imagen));

        public async Task<PersonaResponse> ObtenerPorId(int id)
        {
            Persona actor = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún actor con ese id.");
            return Mapear (actor);
        }

        public async Task<PersonaResponse> Crear(CrearPersonaRequest dto)
        {
            ValidarCampos(dto.Nombre);

            Persona persona = new()
            {
                Nombre = dto.Nombre.Trim(),
            };

            await _repo.Crear(persona);
            return Mapear (persona);
        }

        public async Task Actualizar(int id, ActualizarPersonaRequest dto)
        {
            ValidarCampos(dto.Nombre);

            Persona persona = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún actor con ese id.");

            persona.Nombre = dto.Nombre.Trim();

            await _repo.Actualizar(persona);
        }

        public async Task Eliminar(int id)
        {
            Persona persona = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún actor con ese id.");

            if (await _repo.TienePeliculasAsociadas(id))
                throw new RecursoExistenteException("No se puede eliminar: hay películas con este actor.");

            await _repo.Eliminar(persona);
        }

        private static void ValidarCampos(string nombre)
        {
            if (!Validaciones.EstanDatosBien(nombre))
                throw new DatosLlegaronErradosException("El nombre del actor es obligatorio.");
            if (nombre.Trim().Length > 200)
                throw new DatosLlegaronErradosException("El nombre no puede superar 200 caracteres.");
        }
    }
}
