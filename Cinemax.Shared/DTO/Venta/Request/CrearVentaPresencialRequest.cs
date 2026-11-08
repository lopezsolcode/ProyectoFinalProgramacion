using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Venta.Request;
public record CrearVentaPresencialRequest(
    int? FuncionId, 
    List<int> ButacasIds, 
    List<VentaSnackItemRequest> Items,
    MetodoPago Metodo, 
    string? ReferenciaPago, 
    int? ClienteId);