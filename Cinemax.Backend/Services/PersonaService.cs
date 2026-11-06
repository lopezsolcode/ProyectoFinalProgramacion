using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.Cinemax.Backend.Models;
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
        public PersonaService(IPersonaRepository repo) => _repo = repo;

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
            var datos = r.Datos.Select(a => new PersonaResponse(a.Id, a.Nombre, a.ImagenUrl)).ToList();
            return new PaginadoResponse<PersonaResponse>(datos, r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<PersonaResponse> ObtenerPorId(int id)
        {
            Persona actor = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún actor con ese id.");
            return new PersonaResponse(actor.Id, actor.Nombre, actor.ImagenUrl);
        }

        public async Task<PersonaResponse> Crear(CrearPersonaRequest dto)
        {
            ValidarCampos(dto.Nombre, dto.ImagenUrl);

            Persona persona = new()
            {
                Nombre = dto.Nombre.Trim(),
                ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim()
            };

            await _repo.Crear(persona);
            return new PersonaResponse(persona.Id, persona.Nombre, persona.ImagenUrl);
        }

        public async Task Actualizar(int id, ActualizarPersonaRequest dto)
        {
            ValidarCampos(dto.Nombre, dto.ImagenUrl);

            Persona persona = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún actor con ese id.");

            persona.Nombre = dto.Nombre.Trim();
            persona.ImagenUrl = string.IsNullOrWhiteSpace(dto.ImagenUrl) ? null : dto.ImagenUrl.Trim();

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

        private static void ValidarCampos(string nombre, string? imagenUrl)
        {
            if (!Validaciones.EstanDatosBien(nombre))
                throw new DatosLlegaronErradosException("El nombre del actor es obligatorio.");
            if (nombre.Trim().Length > 200)
                throw new DatosLlegaronErradosException("El nombre no puede superar 200 caracteres.");

            if (!string.IsNullOrWhiteSpace(imagenUrl))
            {
                if (imagenUrl.Trim().Length > 500)
                    throw new DatosLlegaronErradosException("La URL de la imagen no puede superar 500 caracteres.");

                bool valida = Uri.TryCreate(imagenUrl.Trim(), UriKind.Absolute, out Uri? uri)
                              && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
                if (!valida)
                    throw new DatosLlegaronErradosException("La URL de la imagen no es válida.");
            }
        }
    }
}
