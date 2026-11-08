using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Paginado.Request.Snack;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Snack.Request;
using Cinemax.Shared.DTO.Snack.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cinemax.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SnacksController : ControllerBase
    {
        private readonly SnackService _service;
        public SnacksController(SnackService service) => _service = service;

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PaginadoResponseDto<SnackResponse>>> ObtenerTodos(
            [FromQuery] ParametroPaginacionSnackRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerTodos(parametros);
                return Ok(new PaginadoResponseDto<SnackResponse>(r.NumeroPagina, r.TamanoPagina,
                    r.TotalRegistros, r.TotalPaginas, r.TienePaginaAnterior, r.TienePaginaPosterior, r.Datos));
            }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpGet("catalogo")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginadoResponseDto<SnackCatalogoResponse>>> ObtenerCatalogo(
            [FromQuery] ParametroPaginacionSnackRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerCatalogo(parametros);
                return Ok(new PaginadoResponseDto<SnackCatalogoResponse>(r.NumeroPagina, r.TamanoPagina,
                    r.TotalRegistros, r.TotalPaginas, r.TienePaginaAnterior, r.TienePaginaPosterior, r.Datos));
            }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<SnackResponse>> ObtenerPorId(int id)
        {
            try { return Ok(await _service.ObtenerPorId(id)); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }


        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<SnackResponse>> Crear(CrearSnackRequest dto)
        {
            try
            {
                SnackResponse r = await _service.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = r.Id }, r);
            }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, ActualizarSnackRequest dto)
        {
            try { await _service.Actualizar(id, dto); return NoContent(); }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (RecursoExistenteException e) { return Conflict(e.Message); }
            catch (BaseDeDatosException e) { return StatusCode(StatusCodes.Status500InternalServerError, e.Message); }
        }

        [HttpPost("{id}/stock")]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<SnackResponse>> AjustarStock(int id, AjustarStockRequest dto)
        {
            try { return Ok(await _service.AjustarStock(id, dto)); }
            catch (DatosLlegaronErradosException e) { return BadRequest(e.Message); }
            catch (RecursoNoExisteException e) { return NotFound(e.Message); }
            catch (StockInsuficienteException e) { return Conflict(e.Message); }
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
