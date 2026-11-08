using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinemax.Backend.Data.Configuraciones
{
    // Data/Configuraciones
    public class EntradaVentaConfiguracion : IEntityTypeConfiguration<EntradaVenta>
    {
        public void Configure(EntityTypeBuilder<EntradaVenta> builder)
        {
            builder.ToTable("EntradasVenta");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Precio).HasPrecision(12, 2);

            builder.HasOne(x => x.Venta).WithMany(x => x.Entradas)
                .HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Butaca).WithMany()
                .HasForeignKey(x => x.ButacaId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Funcion>().WithMany()
                .HasForeignKey(x => x.FuncionId).OnDelete(DeleteBehavior.Restrict);

            // Esto impide vender dos veces la misma butaca. Es la defensa real contra la carrera.
            builder.HasIndex(x => new { x.FuncionId, x.ButacaId })
                .IsUnique()
                .HasFilter("\"Activa\" = true")
                .HasDatabaseName("UX_EntradaVenta_Funcion_Butaca_Activa");

            builder.HasIndex(x => new { x.VentaId, x.ButacaId }).IsUnique();
        }
    }

    public class DetalleVentaSnackConfiguracion : IEntityTypeConfiguration<DetalleVentaSnack>
    {
        public void Configure(EntityTypeBuilder<DetalleVentaSnack> builder)
        {
            builder.ToTable("DetallesVentaSnack", t => t.HasCheckConstraint("CK_DetalleVentaSnack_SnackOCombo",
                "(\"SnackId\" IS NOT NULL AND \"ComboId\" IS NULL) OR (\"SnackId\" IS NULL AND \"ComboId\" IS NOT NULL)"));
            builder.HasKey(x => x.Id);
            builder.Property(x => x.PrecioUnitario).HasPrecision(12, 2);

            builder.HasOne(x => x.Venta).WithMany(x => x.Snacks)
                .HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Snack).WithMany()
                .HasForeignKey(x => x.SnackId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(x => x.Combo).WithMany()
                .HasForeignKey(x => x.ComboId).OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ConsumoStockVentaConfiguracion : IEntityTypeConfiguration<ConsumoStockVenta>
    {
        public void Configure(EntityTypeBuilder<ConsumoStockVenta> builder)
        {
            builder.ToTable("ConsumosStockVenta");
            builder.HasKey(x => x.Id);
            builder.HasOne<Venta>().WithMany(v => v.ConsumosStock)
                .HasForeignKey(x => x.VentaId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Snack>().WithMany()
                .HasForeignKey(x => x.SnackId).OnDelete(DeleteBehavior.Restrict);
            builder.HasIndex(x => new { x.VentaId, x.SnackId }).IsUnique();
        }
    }
}
