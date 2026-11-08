using Cinemax.Shared.DTO.Comun;
namespace Cinemax.Shared.DTO.Combo.Response
{
    public record ComboItemResponse(
        int SnackId, 
        string SnackNombre, 
        int Cantidad);
    public record ComboResponse(
        int Id, 
        string Nombre, 
        decimal Precio, 
        ImagenResponse? Imagen,
        bool Disponible, 
        List<ComboItemResponse> Items);
}
