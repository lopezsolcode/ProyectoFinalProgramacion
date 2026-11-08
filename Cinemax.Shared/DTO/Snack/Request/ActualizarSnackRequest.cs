namespace Cinemax.Shared.DTO.Snack.Request
{
    public record ActualizarSnackRequest(
        string Nombre, 
        decimal Precio,  
        string DescripcionUnidad);
}
