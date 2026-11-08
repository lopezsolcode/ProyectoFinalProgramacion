using Cinemax.Shared.DTO.Venta.Response;
using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.PDFs.Response;

public record PagoComprobanteResponse(decimal Monto, MetodoPago Metodo, string? ReferenciaPago, EstadoPago Estado);

public class ComprobanteVentaResponse
{
    public VentaComprobanteResponse Venta { get; set; } = null!;
    public List<PagoComprobanteResponse> Pagos { get; set; } = new();
}

public class VentaComprobanteResponse
{
    public int Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Estado { get; set; } = string.Empty;
    public string? ClienteNombre { get; set; }
    public decimal Total { get; set; }

    // Información de la función (opcional)
    public string? FuncionDescripcion { get; set; }

    // Butacas reservadas
    public List<ButacaComprobanteResponse> Butacas { get; set; } = new();

    // Snacks y combos comprados
    public List<SnackComprobanteResponse> Snacks { get; set; } = new();
}

public class ButacaComprobanteResponse
{
    public string Identificacion { get; set; } = string.Empty;
    public decimal Precio { get; set; }
}

public class SnackComprobanteResponse
{
    public string ProductoNombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}