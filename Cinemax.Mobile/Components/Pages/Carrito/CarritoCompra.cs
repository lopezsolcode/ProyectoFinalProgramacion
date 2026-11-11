using System.Text.Json.Serialization;
using Cinemax.Shared.Enums;

namespace Cinemax.Mobile.Models;

// Lo que el cliente va armando: película, función, butacas y snacks. Se guarda por usuario en el celular.
public class CarritoCompra
{
    public PeliculaCarrito? Pelicula { get; set; }
    public FuncionCarrito? Funcion { get; set; }
    public List<ButacaCarrito> Butacas { get; set; } = new();
    public List<LineaCarrito> Lineas { get; set; } = new();
}

public class PeliculaCarrito
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? PosterUrl { get; set; }
}

public class FuncionCarrito
{
    public int Id { get; set; }
    public DateTime FechaHoraUtc { get; set; }
    public int SalaNumero { get; set; }
    public FormatoFuncion Formato { get; set; }
    public IdiomaFuncion Idioma { get; set; }
    public decimal PrecioEntrada { get; set; }
}

public class ButacaCarrito
{
    public int Id { get; set; }
    public string Fila { get; set; } = string.Empty;
    public int Numero { get; set; }

    [JsonIgnore]
    public string Etiqueta => $"{Fila}{Numero}";
}

public class LineaCarrito
{
    public int? SnackId { get; set; }
    public int? ComboId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }

    [JsonIgnore]
    public decimal Subtotal => PrecioUnitario * Cantidad;
}
