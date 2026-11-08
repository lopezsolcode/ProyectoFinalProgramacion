using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinemax.Backend.Data.Configuraciones
{
    public class SnackConfiguracion : IEntityTypeConfiguration<Snack>
    {
        public void Configure(EntityTypeBuilder<Snack> builder)
        {
            builder.ToTable("Snacks");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Precio).HasPrecision(12, 2);

            builder.HasIndex(x => x.Nombre).IsUnique().HasFilter("\"Eliminado\" = false");

            // 1 a 1: EF crea el índice único sobre Inventarios.SnackId.
            builder.HasOne(x => x.Inventario).WithOne(x => x.Snack)
                .HasForeignKey<Inventario>(x => x.SnackId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class InventarioConfiguracion : IEntityTypeConfiguration<Inventario>
    {
        public void Configure(EntityTypeBuilder<Inventario> builder)
        {
            builder.ToTable("Inventarios", t =>
                t.HasCheckConstraint("CK_Inventario_Cantidad", "\"CantidadDisponible\" >= 0"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.DescripcionUnidad).IsRequired().HasMaxLength(50);
        }
    }

    public class ComboConfiguracion : IEntityTypeConfiguration<Combo>
    {
        public void Configure(EntityTypeBuilder<Combo> builder)
        {
            builder.ToTable("Combos");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Precio).HasPrecision(12, 2);

            builder.HasIndex(x => x.Nombre).IsUnique().HasFilter("\"Eliminado\" = false");
        }
    }

    public class ComboSnackConfiguracion : IEntityTypeConfiguration<ComboSnack>
    {
        public void Configure(EntityTypeBuilder<ComboSnack> builder)
        {
            builder.ToTable("ComboSnacks", t =>
                t.HasCheckConstraint("CK_ComboSnack_Cantidad", "\"Cantidad\" > 0"));
            builder.HasKey(x => x.Id);

            // Los componentes son parte del combo: se borran con él.
            builder.HasOne(x => x.Combo).WithMany(x => x.Componentes)
                .HasForeignKey(x => x.ComboId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Snack).WithMany(x => x.Combos)
                .HasForeignKey(x => x.SnackId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.ComboId, x.SnackId }).IsUnique();
        }
    }
}
