using Cinemax.Shared.DTO.Comun;
namespace Cinemax.Shared.DTO.Persona.Response
{
    public record PersonaResponse(int Id, string Nombre, ImagenResponse? Imagen);
}
