using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinemax.Backend.Data.Configuraciones
{
    public class SalaConfiguracion : IEntityTypeConfiguration<Sala>
    {
        public void Configure(EntityTypeBuilder<Sala> builder)
        {
            builder.ToTable("Salas");
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.Numero)
                .IsUnique()
                .HasFilter("\"Eliminado\" = false");
        }
    }

    public class ButacaConfiguracion : IEntityTypeConfiguration<Butaca>
    {
        public void Configure(EntityTypeBuilder<Butaca> builder)
        {
            builder.ToTable("Butacas");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Fila).IsRequired().HasMaxLength(5);

            builder.HasOne(x => x.Sala).WithMany(x => x.Butacas)
                .HasForeignKey(x => x.SalaId).OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(x => new { x.SalaId, x.Fila, x.Numero }).IsUnique();
            builder.HasIndex(x => new { x.SalaId, x.Fila, x.Columna }).IsUnique();
        }
    }
}
