using Cinemax.Shared.DTO.Funcion.Response;
using Cinemax.Shared.DTO.Pelicula.Response;
using Cinemax.Shared.DTO.Venta.Response;

namespace Cinemax.Web.Services;

public enum PasoPos
{
    Pelicula,
    Funcion,
    Butacas,
    Snacks,
    Cobro,
    Finalizada
}

public class LineaSnackPos
{
    public int? SnackId { get; init; }
    public int? ComboId { get; init; }
    public required string Nombre { get; init; }
    public decimal PrecioUnitario { get; init; }
    public int Cantidad { get; set; }

    public decimal Subtotal => PrecioUnitario * Cantidad;
}

// Estado de la venta en curso. Es scoped: lo comparten el panel izquierdo (pasos) y el derecho (detalle).
public class PosEstadoVenta
{
    public const int MaxCantidadPorItem = 50;

    private readonly List<ButacaFuncionResponse> _butacas = new();
    private readonly List<LineaSnackPos> _lineas = new();

    public event Action? Cambio;

    public PasoPos Paso { get; private set; } = PasoPos.Pelicula;
    public PeliculaCatalogoResponse? Pelicula { get; private set; }
    public FuncionCatalogoResponse? Funcion { get; private set; }
    public VentaResponse? VentaFinalizada { get; private set; }

    public IReadOnlyList<ButacaFuncionResponse> Butacas => _butacas;
    public IReadOnlyList<LineaSnackPos> Lineas => _lineas;

    public decimal TotalEntradas => Funcion is null ? 0 : Funcion.PrecioEntrada * _butacas.Count;
    public decimal TotalSnacks => _lineas.Sum(l => l.Subtotal);
    public decimal Total => TotalEntradas + TotalSnacks;
    public bool HayItems => _butacas.Count > 0 || _lineas.Count > 0;

    // ---- Navegación ----

    public bool PuedeIr(PasoPos paso) => paso switch
    {
        PasoPos.Pelicula => true,
        PasoPos.Funcion => Pelicula is not null,
        PasoPos.Butacas => Funcion is not null,
        PasoPos.Snacks => true,
        PasoPos.Cobro => HayItems,
        _ => false
    };

    public void IrA(PasoPos paso)
    {
        if (!PuedeIr(paso)) return;
        Paso = paso;
        Cambio?.Invoke();
    }

    // ---- Película, función y butacas ----

    public void ElegirPelicula(PeliculaCatalogoResponse pelicula)
    {
        if (Pelicula?.Id != pelicula.Id)
        {
            Funcion = null;
            _butacas.Clear();
        }

        Pelicula = pelicula;
        Paso = PasoPos.Funcion;
        Cambio?.Invoke();
    }

    // Venta solo de snacks: se descartan película, función y butacas.
    public void SoloSnacks()
    {
        Pelicula = null;
        Funcion = null;
        _butacas.Clear();
        Paso = PasoPos.Snacks;
        Cambio?.Invoke();
    }

    public void ElegirFuncion(FuncionCatalogoResponse funcion)
    {
        if (Funcion?.Id != funcion.Id)
        {
            _butacas.Clear();
        }

        Funcion = funcion;
        Paso = PasoPos.Butacas;
        Cambio?.Invoke();
    }

    public bool EstaSeleccionada(int butacaId) => _butacas.Any(b => b.Id == butacaId);

    // Devuelve false si se alcanzó el máximo de butacas por venta.
    public bool AlternarButaca(ButacaFuncionResponse butaca, int maximo)
    {
        int indice = _butacas.FindIndex(b => b.Id == butaca.Id);

        if (indice >= 0)
        {
            _butacas.RemoveAt(indice);
        }
        else
        {
            if (_butacas.Count >= maximo) return false;
            _butacas.Add(butaca);
        }

        Cambio?.Invoke();
        return true;
    }

    public void QuitarButaca(int butacaId) => DescartarButacas(new[] { butacaId });

    public void DescartarButacas(IEnumerable<int> ids)
    {
        HashSet<int> aQuitar = ids.ToHashSet();

        if (_butacas.RemoveAll(b => aQuitar.Contains(b.Id)) > 0)
        {
            Cambio?.Invoke();
        }
    }

    // ---- Snacks y combos ----

    public int CantidadDe(int? snackId, int? comboId) =>
        _lineas.FirstOrDefault(l => l.SnackId == snackId && l.ComboId == comboId)?.Cantidad ?? 0;

    public void AgregarSnack(int snackId, string nombre, decimal precio) =>
        Agregar(snackId, null, nombre, precio);

    public void AgregarCombo(int comboId, string nombre, decimal precio) =>
        Agregar(null, comboId, nombre, precio);

    private void Agregar(int? snackId, int? comboId, string nombre, decimal precio)
    {
        LineaSnackPos? linea = _lineas.FirstOrDefault(l => l.SnackId == snackId && l.ComboId == comboId);

        if (linea is null)
        {
            _lineas.Add(new LineaSnackPos
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

        Cambio?.Invoke();
    }

    public void CambiarCantidad(LineaSnackPos linea, int delta)
    {
        int nueva = linea.Cantidad + delta;

        if (nueva < 1) _lineas.Remove(linea);
        else if (nueva <= MaxCantidadPorItem) linea.Cantidad = nueva;

        Cambio?.Invoke();
    }

    public void QuitarLinea(LineaSnackPos linea)
    {
        if (_lineas.Remove(linea)) Cambio?.Invoke();
    }

    // ---- Cierre ----

    public void Finalizar(VentaResponse venta)
    {
        VentaFinalizada = venta;
        Paso = PasoPos.Finalizada;
        Cambio?.Invoke();
    }

    public void Reiniciar()
    {
        Pelicula = null;
        Funcion = null;
        VentaFinalizada = null;
        _butacas.Clear();
        _lineas.Clear();
        Paso = PasoPos.Pelicula;
        Cambio?.Invoke();
    }
}
