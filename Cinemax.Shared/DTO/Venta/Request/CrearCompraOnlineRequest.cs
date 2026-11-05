namespace Cinemax.Shared.DTO.Venta.Request;

public record CrearCompraOnlineRequest(
    List<CrearVentaItemRequest> Items
);
