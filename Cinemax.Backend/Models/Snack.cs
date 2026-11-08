namespace Cinemax.Backend.Models
{
        public class Snack
        {
            public int Id { get; set; }
            public required string Nombre { get; set; }
            public decimal Precio { get; set; }
            public Imagen? Imagen { get; set; }
            public bool Eliminado { get; set; } = false;
            public Inventario Inventario { get; set; } = null!;
            public ICollection<ComboSnack> Combos { get; set; } = new List<ComboSnack>();
        }

        public class Inventario
        {
            public int Id { get; set; }
            public int SnackId { get; set; }
            public required string DescripcionUnidad { get; set; }  // "unidad", "vaso 500 ml"
            public int CantidadDisponible { get; set; }
            public Snack Snack { get; set; } = null!;
        }

        public class Combo
        {
            public int Id { get; set; }
            public required string Nombre { get; set; }
            public decimal Precio { get; set; }          // precio propio, no es la suma de los componentes
            public Imagen? Imagen { get; set; }
            public bool Eliminado { get; set; } = false;
            public ICollection<ComboSnack> Componentes { get; set; } = new List<ComboSnack>();
        }

        public class ComboSnack
        {
            public int Id { get; set; }
            public int ComboId { get; set; }
            public int SnackId { get; set; }
            public int Cantidad { get; set; }
            public Combo Combo { get; set; } = null!;
            public Snack Snack { get; set; } = null!;
        }
}
