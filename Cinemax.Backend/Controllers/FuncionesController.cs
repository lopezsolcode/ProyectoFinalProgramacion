using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Funcion.Request;
using Cinemax.Shared.DTO.Funcion.Response;
using Cinemax.Shared.DTO.Paginado.Request.Funcion;
using Cinemax.Shared.DTO.Paginado.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinemax.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FuncionesController : ControllerBase
    {
        private readonly FuncionService _service;
        public FuncionesController(FuncionService service) => _service = service;

        // Admin: todas, pasadas y futuras.
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PaginadoResponseDto<FuncionResponse>>> ObtenerTodos(
            [FromQuery] ParametroPaginacionFuncionRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerTodos(parametros);
                return Ok(new PaginadoResponseDto<FuncionResponse>(r.NumeroPagina, r.TamanoPagina,
                    r.TotalRegistros, r.TotalPaginas, r.TienePaginaAnterior, r.TienePaginaPosterior, r.Datos));
            }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        // Público y POS: solo funciones futuras.
        [HttpGet("disponibles")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginadoResponseDto<FuncionCatalogoResponse>>> ObtenerDisponibles(
            [FromQuery] ParametroPaginacionFuncionRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerDisponibles(parametros);
                return Ok(new PaginadoResponseDto<FuncionCatalogoResponse>(r.NumeroPagina, r.TamanoPagina,
                    r.TotalRegistros, r.TotalPaginas, r.TienePaginaAnterior, r.TienePaginaPosterior, r.Datos));
            }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<FuncionResponse>> ObtenerPorId(int id)
        {
            try { return Ok(await _service.ObtenerPorId(id)); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpGet("{id}/mapa")]
        [AllowAnonymous]
        public async Task<ActionResult<MapaFuncionResponse>> ObtenerMapa(int id)
        {
            try { return Ok(await _service.ObtenerMapa(id)); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<FuncionResponse>> Crear(CrearFuncionRequest dto)
        {
            try
            {
                FuncionResponse r = await _service.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = r.Id }, r);
            }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, ActualizarFuncionRequest dto)
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
