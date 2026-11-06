using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Genero.Request;
using Cinemax.Shared.DTO.Genero.Response;
using Cinemax.Shared.DTO.Paginado.Request.Genero;
using Cinemax.Shared.DTO.Paginado.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinemax.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerosController : ControllerBase
    {
        private readonly GeneroService _service;
        public GenerosController(GeneroService service) => _service = service;

        [HttpGet]
        [AllowAnonymous] // lo usa el filtro de la cartelera
        public async Task<ActionResult<PaginadoResponseDto<GeneroResponse>>> ObtenerTodos(
            [FromQuery] ParametroPaginacionGeneroRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerTodos(parametros);
                return Ok(new PaginadoResponseDto<GeneroResponse>(r.NumeroPagina, r.TamanoPagina,
                    r.TotalRegistros, r.TotalPaginas, r.TienePaginaAnterior, r.TienePaginaPosterior, r.Datos));
            }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<GeneroResponse>> ObtenerPorId(int id)
        {
            try { return Ok(await _service.ObtenerPorId(id)); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<GeneroResponse>> Crear(CrearGeneroRequest dto)
        {
            try
            {
                GeneroResponse r = await _service.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = r.Id }, r);
            }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, ActualizarGeneroRequest dto)
        {
            try { await _service.Actualizar(id, dto); return NoContent(); }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
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
