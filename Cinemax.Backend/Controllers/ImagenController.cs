using Cinemax.Backend.Models;
using Cinemax.Backend.Services;
using Cinemax.Shared.DTO.Comun;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cinemax.Backend.Utilidades;
namespace Cinemax.Backend.Controllers
{
    [ApiController]
    [Route("api/imagenes")]
    [Authorize(Roles = "Administrador")]
    public class ImagenController : ControllerBase
    {
        private readonly ImagenService _imagenService;
        private readonly ImagenMapeador _mapeador;

        public ImagenController(ImagenService imagenService, ImagenMapeador mapeador)
        {
            _imagenService = imagenService;
            _mapeador = mapeador;
        }


        // POST: api/imagenes/snacks/5
        [HttpPost("snacks/{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImagenResponse>> SubirImagenSnack(
            int id,
            [FromForm] IFormFile archivo)
        {
            Imagen imagen = await _imagenService.SubirImagenSnack(id, archivo);

            return Ok(_mapeador.Mapear(imagen));
        }

        // POST: api/imagenes/combos/5
        [HttpPost("combos/{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImagenResponse>> SubirImagenCombo(
            int id,
            [FromForm] IFormFile archivo)
        {
            Imagen imagen = await _imagenService.SubirImagenCombo(id, archivo);

            return Ok(_mapeador.Mapear(imagen)); ;
        }

        // POST: api/imagenes/personas/5
        [HttpPost("personas/{id:int}")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImagenResponse>> SubirImagenPersona(
            int id,
            [FromForm] IFormFile archivo)
        {
            Imagen imagen = await _imagenService.SubirImagenPersona(id, archivo);

            return Ok(_mapeador.Mapear(imagen)); ;
        }

        // POST: api/imagenes/peliculas/5/principal
        [HttpPost("peliculas/{id:int}/principal")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImagenResponse>> SubirImagenPrincipalPelicula(
            int id,
            [FromForm] IFormFile archivo)
        {
            Imagen imagen = await _imagenService.SubirImagenPrincipalPelicula(
                id,
                archivo);

            return Ok(_mapeador.Mapear(imagen)); ;
        }

        // POST: api/imagenes/peliculas/5/poster
        [HttpPost("peliculas/{id:int}/poster")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ImagenResponse>> SubirPosterPelicula(
            int id,
            [FromForm] IFormFile archivo)
        {
            Imagen imagen = await _imagenService.SubirPosterPelicula(
                id,
                archivo);

            return Ok(_mapeador.Mapear(imagen)); ;
        }
    }
}
