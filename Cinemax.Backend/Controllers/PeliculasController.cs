using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Paginado.Request.Pelicula;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Pelicula.Request;
using Cinemax.Shared.DTO.Pelicula.Response;

namespace Cinemax.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly PeliculaService _service;

        public PeliculasController(PeliculaService service) => _service = service;

        // Público: solo películas en cartelera, sin datos administrativos.
        [HttpGet("cartelera")]
        [AllowAnonymous]
        public async Task<ActionResult<PaginadoResponseDto<PeliculaCatalogoResponse>>> ObtenerCartelera(
            [FromQuery] ParametroPaginacionPeliculaRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerCartelera(parametros);

                return Ok(new PaginadoResponseDto<PeliculaCatalogoResponse>(
                    r.NumeroPagina,
                    r.TamanoPagina,
                    r.TotalRegistros,
                    r.TotalPaginas,
                    r.TienePaginaAnterior,
                    r.TienePaginaPosterior,
                    r.Datos));
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        // Administración: todas las películas no eliminadas, estén o no en cartelera.
        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PaginadoResponseDto<PeliculaListadoResponse>>> ObtenerTodos(
            [FromQuery] ParametroPaginacionPeliculaRequest parametros)
        {
            try
            {
                var r = await _service.ObtenerTodos(parametros);

                return Ok(new PaginadoResponseDto<PeliculaListadoResponse>(
                    r.NumeroPagina,
                    r.TamanoPagina,
                    r.TotalRegistros,
                    r.TotalPaginas,
                    r.TienePaginaAnterior,
                    r.TienePaginaPosterior,
                    r.Datos));
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<PeliculaResponse>> ObtenerPorId(int id)
        {
            try
            {
                return Ok(await _service.ObtenerPorId(id));
            }
            catch (RecursoNoExisteException e)
            {
                return NotFound(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<ActionResult<PeliculaResponse>> Crear(CrearPeliculaRequest dto)
        {
            try
            {
                PeliculaResponse r = await _service.Crear(dto);
                return CreatedAtAction(nameof(ObtenerPorId), new { id = r.Id }, r);
            }
            catch (DatosLlegaronErradosException e)
            {
                return BadRequest(e.Message);
            }
            catch (RecursoExistenteException e)
            {
                return Conflict(e.Message);
            }
            catch (RecursoNoExisteException e)
            {
                return NotFound(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Actualizar(int id, ActualizarPeliculaRequest dto)
        {
            try
            {
                await _service.Actualizar(id, dto);
                return NoContent();
            }
            catch (DatosLlegaronErradosException e)
            {
                return BadRequest(e.Message);
            }
            catch (RecursoExistenteException e)
            {
                return Conflict(e.Message);
            }
            catch (RecursoNoExisteException e)
            {
                return NotFound(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                await _service.Eliminar(id);
                return NoContent();
            }
            catch (RecursoNoExisteException e)
            {
                return NotFound(e.Message);
            }
            catch (RecursoExistenteException e)
            {
                // Reservado para cuando exista Función: baja bloqueada si hay funciones futuras.
                return Conflict(e.Message);
            }
            catch (BaseDeDatosException e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}