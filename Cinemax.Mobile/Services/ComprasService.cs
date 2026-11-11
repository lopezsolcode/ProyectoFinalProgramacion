using System.Net.Http.Json;
using Cinemax.Shared.DTO.Paginado.Response;
using Cinemax.Shared.DTO.Venta.Request;
using Cinemax.Shared.DTO.Venta.Response;
using Cinemax.Shared.Enums;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Cinemax.Mobile.Services;

public class ComprasService
{
    private const string SoloClientes = "Solo las cuentas de cliente pueden realizar compras.";

    private readonly AutenticacionService _autenticacionService;

    public ComprasService(AutenticacionService autenticacionService)
    {
        _autenticacionService = autenticacionService;
    }

    // Crea la compra pendiente: reserva las butacas y el stock hasta que venza o se pague.
    public async Task<VentaResponse> CrearCompraOnline(CrearCompraOnlineRequest request)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Post, "api/ventas/online")
            {
                Content = JsonContent.Create(request)
            });

        return await LeerVenta(respuesta, "No se pudo iniciar la compra.");
    }

    // Si el pago es rechazado no hay excepción: la venta vuelve en estado Cancelada y las butacas quedan libres.
    public async Task<VentaResponse> Pagar(int ventaId, MetodoPago metodo)
    {
        PagarVentaRequest request = new(metodo);

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Post, $"api/ventas/{ventaId}/pago")
            {
                Content = JsonContent.Create(request)
            });

        return await LeerVenta(respuesta, "No se pudo procesar el pago.");
    }

    public async Task Cancelar(int ventaId)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Post, $"api/ventas/mis-compras/{ventaId}/cancelar"));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo cancelar la compra.", SoloClientes);
    }

    public async Task<PaginadoResponseDto<VentaListadoResponse>> ObtenerMisCompras(
        int pagina, EstadoVenta? estado = null)
    {
        string ruta = $"api/ventas/mis-compras?NumeroPagina={pagina}&TamanoPagina=10&OrdenarPor=fecha&Direccion=desc";

        if (estado.HasValue)
            ruta += $"&Estado={estado.Value}";

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Get, ruta));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudieron obtener las compras.", SoloClientes);

        return await respuesta.Content.ReadFromJsonAsync<PaginadoResponseDto<VentaListadoResponse>>()
               ?? throw new InvalidOperationException("No se pudieron obtener las compras.");
    }

    public async Task<VentaResponse> ObtenerMiCompra(int ventaId)
    {
        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Get, $"api/ventas/mis-compras/{ventaId}"));

        return await LeerVenta(respuesta, "No se pudo obtener la compra.");
    }

    public async Task<byte[]> ObtenerComprobante(int ventaId)
    {
        string ruta = $"api/ventas/mis-compras/{ventaId}/pdf";

        using HttpResponseMessage respuesta = await _autenticacionService.EnviarAutenticado(() =>
            new HttpRequestMessage(HttpMethod.Get, ruta));

        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, "No se pudo descargar el comprobante.", SoloClientes);

        return await respuesta.Content.ReadAsByteArrayAsync();
    }

    // Descarga el comprobante y lo abre con la aplicación de PDF del celular. Devuelve false si no hay ninguna.
    public async Task<bool> AbrirComprobante(int ventaId)
    {
        byte[] pdf = await ObtenerComprobante(ventaId);

        string ruta = Path.Combine(FileSystem.CacheDirectory, $"Compra-{ventaId}.pdf");
        await File.WriteAllBytesAsync(ruta, pdf);

        return await Launcher.Default.OpenAsync(
            new OpenFileRequest("Comprobante", new ReadOnlyFile(ruta, "application/pdf")));
    }

    private static async Task<VentaResponse> LeerVenta(HttpResponseMessage respuesta, string mensajePorDefecto)
    {
        if (!respuesta.IsSuccessStatusCode)
            throw await ErrorApi.De(respuesta, mensajePorDefecto, SoloClientes);

        return await respuesta.Content.ReadFromJsonAsync<VentaResponse>()
               ?? throw new InvalidOperationException("El servidor no devolvió los datos de la compra.");
    }
}
