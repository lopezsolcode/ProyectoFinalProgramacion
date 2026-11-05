using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Cliente.Request;
using Cinemax.Shared.DTO.Cliente.Response;
using Cinemax.Shared.DTO.Paginado.Request.Cliente;
using Cinemax.Shared.DTO.Paginado.Response;

namespace Cinemax.Backend.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]

public class ClientesController : ControllerBase
{
    private readonly ClienteService _clienteService;

    public ClientesController(ClienteService clienteService)
    {
        _clienteService = clienteService;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Cajero")]
    public async Task<ActionResult<PaginadoResponseDto<ClienteResponse>>> ObtenerTodos(
        [FromQuery] ParametroPaginacionClienteRequest parametros)
    {
        try
        {
            PaginadoResponse<ClienteResponse> clientes = await _clienteService.ObtenerTodos(parametros);

            PaginadoResponseDto<ClienteResponse> paginadoResponseDto = new PaginadoResponseDto<ClienteResponse>(clientes.NumeroPagina,
                clientes.TamanoPagina, clientes.TotalRegistros, clientes.TotalPaginas, clientes.TienePaginaAnterior,
                clientes.TienePaginaPosterior, clientes.Datos);

            return Ok(paginadoResponseDto);
        }
        catch (BaseDeDatosException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "Administrador,Cajero")]
    public async Task<ActionResult<ClienteResponse>> ObtenerPorId(int id)
    {
        try
        {
            return Ok(await _clienteService.ObtenerPorId(id));
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
    [Authorize(Roles = "Administrador,Cajero")]
    public async Task<ActionResult<ClienteResponse>> Crear(CrearClienteRequest dto)
    {
        try
        {
            ClienteResponse cliente = await _clienteService.Crear(dto);
            return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id }, cliente);
        }
        catch (DatosLlegaronErradosException e)
        {
            return BadRequest(e.Message);
        }
        catch (RecursoExistenteException e)
        {
            return Conflict(e.Message);
        }
        catch (BaseDeDatosException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Administrador,Cajero")] //los cajeros pueden dar de alta y modificar clientes
    public async Task<IActionResult> Actualizar(int id, ActualizarClienteRequest dto)
    {
        try
        {
            await _clienteService.Actualizar(id, dto);
            return NoContent();
        }
        catch (DatosLlegaronErradosException e)
        {
            return BadRequest(e.Message);
        }
        catch (RecursoNoExisteException e)
        {
            return NotFound(e.Message);
        }
        catch (RecursoExistenteException e)
        {
            return Conflict(e.Message);
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
            await _clienteService.Eliminar(id);
            return NoContent();
        }
        catch (RecursoNoExisteException e)
        {
            return NotFound(e.Message);
        }
        catch (RecursoExistenteException e)
        {
            return Conflict(e.Message);
        }
        catch (BaseDeDatosException e)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        }
    }
}

