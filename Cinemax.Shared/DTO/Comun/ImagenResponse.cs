namespace Cinemax.Shared.DTO.Comun
{
    public record ImagenResponse(
        int Id,
        string NombreOriginal,
        string Url,
        string TipoContenido,
        long TamanoBytes,
        DateTime FechaCreacion
    );
}
