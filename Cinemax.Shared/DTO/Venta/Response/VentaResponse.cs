using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Venta.Response;

public record VentaFuncionResponse(
    int FuncionId, 
    int PeliculaId, 
    string PeliculaTitulo, 
    int SalaNumero, 
    DateTime FechaHora);
public record VentaListadoResponse(
    int Id,
    DateTime Fecha,
    decimal Total,
    int? ClienteId,
    string? ClienteNombre,
    EstadoVenta Estado);
public record VentaEntradaResponse(
    int ButacaId, 
    string Fila, 
    int Numero, 
    decimal Precio);
public record VentaSnackResponse(
    int? SnackId, 
    int? ComboId, 
    string Nombre, 
    int Cantidad, 
    decimal PrecioUnitario, 
    decimal Subtotal);

public record VentaResponse(
    int Id,
    DateTime Fecha,
    DateTime? FechaExpiracion, 
    decimal Total,
    int? ClienteId, 
    string? ClienteNombre, 
    int? UsuarioId, 
    string? UsuarioUsername,
    EstadoVenta Estado, 
    VentaFuncionResponse? Funcion,
    List<VentaEntradaResponse> Entradas, 
    List<VentaSnackResponse> Snacks);
//COMPROBANTEVENTARESPONSE??