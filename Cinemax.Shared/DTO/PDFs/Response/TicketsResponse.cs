using Cinemax.Shared.DTO.Venta.Response;
using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.PDFs.Response;

public record PagoComprobanteResponse(decimal Monto, MetodoPago Metodo, string? ReferenciaPago, EstadoPago Estado);

public record ComprobanteVentaResponse(VentaResponse Venta, List<PagoComprobanteResponse> Pagos);