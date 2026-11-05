using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Venta.Request;

public record CrearVentaPresencialRequest(
    int? ClienteId,
    List<CrearVentaItemRequest> Items,
    MetodoPago Metodo,
    string? ReferenciaPago
);