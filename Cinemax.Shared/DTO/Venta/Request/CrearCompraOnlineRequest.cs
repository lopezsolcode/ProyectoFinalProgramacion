namespace Cinemax.Shared.DTO.Venta.Request;

public record CrearCompraOnlineRequest(
    int FuncionId, 
    List<int> ButacasIds, 
    List<VentaSnackItemRequest> Items);
