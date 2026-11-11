using System.Text.Json;
using Cinemax.Mobile.Models;
using Microsoft.AspNetCore.Components.Authorization;

namespace Cinemax.Mobile.Services;

// Guarda la compra en curso. En memoria vive mientras se navega entre pantallas; si hay sesión,
// además se guarda por usuario en el celular, para que dos personas en el mismo teléfono no compartan carrito.
public class CarritoService
{
    // Debe coincidir con Ventas:MaxButacasPorVenta del backend.
    public const int MaxButacas = 10;
    public const int MaxCantidadPorItem = 50;

    private readonly AuthenticationStateProvider _estadoAutenticacion;

    private CarritoCompra _carrito = new();
    private int? _usuarioId; // dueño del carrito en memoria; null = visitante sin sesión
    private bool _inicializado;

    public CarritoService(AuthenticationStateProvider estadoAutenticacion)
    {
        _estadoAutenticacion = estadoAutenticacion;
    }

    public CarritoCompra Carrito => _carrito;

    public bool TieneEntradas => _carrito.Funcion is not null && _carrito.Butacas.Count > 0;

    public bool EstaVacio =>
        _carrito.Pelicula is null && _carrito.Funcion is null &&
        _carrito.Butacas.Count == 0 && _carrito.Lineas.Count == 0;

    public decimal TotalEntradas => _carrito.Funcion is null ? 0 : _carrito.Funcion.PrecioEntrada * _carrito.Butacas.Count;
    public decimal TotalSnacks => _carrito.Lineas.Sum(l => l.Subtotal);
    public decimal Total => TotalEntradas + TotalSnacks;

    // Se llama al abrir cada pantalla de compra: sincroniza el carrito con el usuario actual.
    public async Task Inicializar()
    {
        int? actual = await ObtenerUsuarioId();

        if (_inicializado && _usuarioId == actual) return;

        if (_inicializado && _usuarioId is null && actual is not null && !EstaVacio)
        {
            // Quien armó la selección sin sesión acaba de iniciarla: la conserva y pasa a ser suya.
            _usuarioId = actual;
            await Guardar();
        }
        else
        {
            // Primer uso, cambio de usuario o cierre de sesión: se parte del carrito guardado de ese usuario.
            _usuarioId = actual;
            _carrito = actual is null ? new CarritoCompra() : await Cargar(actual.Value);
        }

        _inicializado = true;
    }

    public async Task ElegirPelicula(PeliculaCarrito pelicula)
    {
        await Inicializar();

        if (_carrito.Pelicula?.Id != pelicula.Id)
        {
            _carrito.Funcion = null;
            _carrito.Butacas.Clear();
        }

        _carrito.Pelicula = pelicula;
        await Guardar();
    }

    public async Task ElegirFuncion(FuncionCarrito funcion)
    {
        await Inicializar();

        if (_carrito.Funcion?.Id != funcion.Id)
        {
            _carrito.Butacas.Clear();
        }

        _carrito.Funcion = funcion;
        await Guardar();
    }

    // La función ya no existe o ya empezó: se descartan película, función y butacas, pero se conservan los snacks.
    public async Task DescartarEntradas()
    {
        await Inicializar();

        _carrito.Pelicula = null;
        _carrito.Funcion = null;
        _carrito.Butacas.Clear();
        await Guardar();
    }

    public bool EstaSeleccionada(int butacaId) => _carrito.Butacas.Any(b => b.Id == butacaId);

    // Devuelve false si se alcanzó el máximo de butacas por compra.
    public async Task<bool> AlternarButaca(ButacaCarrito butaca)
    {
        await Inicializar();

        int indice = _carrito.Butacas.FindIndex(b => b.Id == butaca.Id);

        if (indice >= 0)
        {
            _carrito.Butacas.RemoveAt(indice);
        }
        else
        {
            if (_carrito.Butacas.Count >= MaxButacas) return false;
            _carrito.Butacas.Add(butaca);
        }

        await Guardar();
        return true;
    }

    public async Task DescartarButacas(IEnumerable<int> ids)
    {
        await Inicializar();

        HashSet<int> aQuitar = ids.ToHashSet();

        if (_carrito.Butacas.RemoveAll(b => aQuitar.Contains(b.Id)) > 0)
        {
            await Guardar();
        }
    }

    public int CantidadDe(int? snackId, int? comboId) =>
        _carrito.Lineas.FirstOrDefault(l => l.SnackId == snackId && l.ComboId == comboId)?.Cantidad ?? 0;

    // Agrega una unidad (o crea la línea si todavía no estaba).
    public async Task Agregar(int? snackId, int? comboId, string nombre, decimal precio)
    {
        await Inicializar();

        LineaCarrito? linea = _carrito.Lineas.FirstOrDefault(l => l.SnackId == snackId && l.ComboId == comboId);

        if (linea is null)
        {
            _carrito.Lineas.Add(new LineaCarrito
            {
                SnackId = snackId,
                ComboId = comboId,
                Nombre = nombre,
                PrecioUnitario = precio,
                Cantidad = 1
            });
        }
        else if (linea.Cantidad < MaxCantidadPorItem)
        {
            linea.Cantidad++;
        }

        await Guardar();
    }

    // Suma o resta unidades; al llegar a cero la línea se quita.
    public async Task CambiarCantidad(int? snackId, int? comboId, int delta)
    {
        await Inicializar();

        LineaCarrito? linea = _carrito.Lineas.FirstOrDefault(l => l.SnackId == snackId && l.ComboId == comboId);

        if (linea is null) return;

        int nueva = linea.Cantidad + delta;

        if (nueva < 1) _carrito.Lineas.Remove(linea);
        else if (nueva <= MaxCantidadPorItem) linea.Cantidad = nueva;

        await Guardar();
    }

    public async Task Vaciar()
    {
        await Inicializar();

        _carrito = new CarritoCompra();

        if (_usuarioId is int id)
        {
            SecureStorage.Default.Remove(Clave(id));
        }
    }

    private async Task<int?> ObtenerUsuarioId()
    {
        AuthenticationState estado = await _estadoAutenticacion.GetAuthenticationStateAsync();
        string? id = estado.User.FindFirst("sub")?.Value;

        return int.TryParse(id, out int usuarioId) && usuarioId > 0 ? usuarioId : null;
    }

    private static string Clave(int usuarioId) => $"compra_{usuarioId}";

    private static async Task<CarritoCompra> Cargar(int usuarioId)
    {
        string? json = await SecureStorage.Default.GetAsync(Clave(usuarioId));

        if (string.IsNullOrWhiteSpace(json)) return new CarritoCompra();

        try
        {
            return JsonSerializer.Deserialize<CarritoCompra>(json) ?? new CarritoCompra();
        }
        catch (JsonException)
        {
            return new CarritoCompra();
        }
    }

    private async Task Guardar()
    {
        if (_usuarioId is null) return;

        await SecureStorage.Default.SetAsync(Clave(_usuarioId.Value), JsonSerializer.Serialize(_carrito));
    }
}
