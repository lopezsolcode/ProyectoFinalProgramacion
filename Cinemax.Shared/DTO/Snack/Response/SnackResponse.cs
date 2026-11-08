using Cinemax.Shared.DTO.Comun;
namespace Cinemax.Shared.DTO.Snack.Response
{
    public record SnackResponse(
        int Id, 
        string Nombre, 
        decimal Precio, 
        ImagenResponse? Imagen,
        string DescripcionUnidad, 
        int CantidadDisponible);
    public record SnackCatalogoResponse(
        int Id, 
        string Nombre, 
        decimal Precio,
        ImagenResponse? Imagen,
        string DescripcionUnidad, 
        bool Disponible);

}
