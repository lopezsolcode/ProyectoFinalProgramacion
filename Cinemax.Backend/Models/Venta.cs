using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Models
{
    // Models/Venta.cs: lo que cambia
    public class Venta
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public decimal Total { get; set; }
        public EstadoVenta Estado { get; set; } = EstadoVenta.Pendiente;
        public int? ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public int? UsuarioId { get; set; }          // cajero; null = compra online
        public Usuario? Usuario { get; set; }
        public int? FuncionId { get; set; }          // null = venta solo de snacks (POS)
        public Funcion? Funcion { get; set; }
        public ICollection<EntradaVenta> Entradas { get; set; } = new List<EntradaVenta>();
        public ICollection<DetalleVentaSnack> Snacks { get; set; } = new List<DetalleVentaSnack>();
        public ICollection<ConsumoStockVenta> ConsumosStock { get; set; } = new List<ConsumoStockVenta>();
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    }

    public class EntradaVenta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int FuncionId { get; set; }       // desnormalizado a propósito: lo necesita el índice único
        public int ButacaId { get; set; }
        public decimal Precio { get; set; }      // congelado al vender
        public bool Activa { get; set; } = true; // false al cancelar, vencer o anular: queda como historial
        public Venta Venta { get; set; } = null!;
        public Butaca Butaca { get; set; } = null!;
    }

    public class DetalleVentaSnack
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int? SnackId { get; set; }
        public int? ComboId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }  // congelado al vender
        public Venta Venta { get; set; } = null!;
        public Snack? Snack { get; set; }
        public Combo? Combo { get; set; }
    }

    public class ConsumoStockVenta
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int SnackId { get; set; }
        public int Cantidad { get; set; }
    }
}
