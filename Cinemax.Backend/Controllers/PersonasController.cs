using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Persona.Response;
using Cinemax.Shared.DTO.Persona.Request;
using Cinemax.Shared.DTO.Paginado.Request.Persona;
using Cinemax.Backend.Excepciones;
using Cinemax.Shared.DTO.Paginado.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinemax.Backend.Controllers
{
[Route("api/[controller]")]
[ApiController]
    public class PersonasController : ControllerBase
    {
        private readonly PersonaService _service;
        public PersonasController(PersonaService service) => _service = service;

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginadoResponseDto<PersonaResponse>>> ObtenerTodos(
            [FromQuery] ParametroPaginacionPersonaRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerTodos(parametros);
                return Ok(new PaginadoResponseDto<PersonaResponse>(r.NumeroPagina, r.TamanoPagina,
                    r.TotalRegistros, r.TotalPaginas, r.TienePaginaAnterior, r.TienePaginaPosterior, r.Datos));
            }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PersonaResponse>> ObtenerPorId(int id)
        {
            try { return Ok(await _service.ObtenerPorId(id)); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PersonaResponse>> Crear(CrearPersonaRequest dto)
        {
            try
            {
                PersonaResponse r = await _service.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = r.Id }, r);
            }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, ActualizarPersonaRequest dto)
        {
            try { await _service.Actualizar(id, dto); return NoContent(); }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try { await _service.Eliminar(id); return NoContent(); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

    }
}