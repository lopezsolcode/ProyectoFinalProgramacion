using System.Text.Json;
using Cinemax.Web.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Cinemax.Web.Components.Pos;

// Base de los componentes del POS: se suscribe al estado de la venta para redibujarse cuando cambia.
public abstract class PosComponentBase : ComponentBase, IDisposable
{
    [Inject] protected PosEstadoVenta Estado { get; set; } = default!;
    [Inject] protected PosService Api { get; set; } = default!;
    [Inject] protected ISnackbar Snackbar { get; set; } = default!;

    protected override void OnInitialized() => Estado.Cambio += Refrescar;

    private void Refrescar() => _ = InvokeAsync(StateHasChanged);

    public virtual void Dispose() => Estado.Cambio -= Refrescar;

    protected static bool EsErrorEsperado(Exception e) =>
        e is HttpRequestException or InvalidOperationException or JsonException or PosVentaException;

    protected static string Mensaje(Exception e, string porDefecto) => e switch
    {
        PosVentaException => e.Message,
        InvalidOperationException => e.Message,
        JsonException => "El servidor devolvió una respuesta inválida.",
        _ => porDefecto
    };
}
