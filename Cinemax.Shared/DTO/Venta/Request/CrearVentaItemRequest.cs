namespace Cinemax.Shared.DTO.Venta.Request;
public record CrearVentaItemRequest(
    int ProductoId,
    int Cantidad
);