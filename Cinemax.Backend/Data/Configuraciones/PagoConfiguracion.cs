using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinemax.Backend.Data.Configuraciones
{
    public class PagoConfiguracion : IEntityTypeConfiguration<Pago>
    {
        public void Configure(EntityTypeBuilder<Pago> builder)
        {
            builder.ToTable("Pagos");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Metodo).IsRequired();
            builder.Property(x => x.Estado).IsRequired();
            builder.Property(x => x.Monto).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.Fecha).IsRequired();
            builder.Property(x => x.ReferenciaPago).HasMaxLength(100);
            builder.Property(x => x.Estado).HasConversion<string>().HasMaxLength(20);
            builder.HasIndex(x => x.VentaId).IsUnique()
                .HasFilter("\"Estado\" = 'Pendiente'")
                .HasDatabaseName("UX_Pago_Venta_Pendiente");

            builder.HasOne(x => x.Venta)
                .WithMany(x => x.Pagos)
                .HasForeignKey(x => x.VentaId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => x.VentaId);
        }
    }

}
