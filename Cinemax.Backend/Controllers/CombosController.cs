using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Combo.Request;
using Cinemax.Shared.DTO.Combo.Response;
using Cinemax.Shared.DTO.Paginado.Request.Combo;
using Cinemax.Shared.DTO.Paginado.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinemax.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CombosController : ControllerBase
    {
        private readonly ComboService _service;
        public CombosController(ComboService service) => _service = service;

        // Público: el combo no tiene datos sensibles y el POS y la app usan el mismo listado.
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<PaginadoResponseDto<ComboResponse>>> ObtenerTodos(
            [FromQuery] ParametroPaginacionComboRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerTodos(parametros);
                return Ok(new PaginadoResponseDto<ComboResponse>(r.NumeroPagina, r.TamanoPagina,
                    r.TotalRegistros, r.TotalPaginas, r.TienePaginaAnterior, r.TienePaginaPosterior, r.Datos));
            }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ComboResponse>> ObtenerPorId(int id)
        {
            try { return Ok(await _service.ObtenerPorId(id)); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<ComboResponse>> Crear(CrearComboRequest dto)
        {
            try
            {
                ComboResponse r = await _service.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = r.Id }, r);
            }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, ActualizarComboRequest dto)
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
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }
    }
}
