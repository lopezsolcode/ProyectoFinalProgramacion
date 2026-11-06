using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinemax.Backend.Data.Configuraciones
{
    public class PeliculaConfiguracion : IEntityTypeConfiguration<Pelicula>
    {
        public void Configure(EntityTypeBuilder<Pelicula> builder)
        {
            builder.ToTable("Peliculas");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Titulo).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Sinopsis).IsRequired().HasMaxLength(2000);
            builder.Property(x => x.Clasificacion).HasConversion<string>().HasMaxLength(20);
            builder.Property(x => x.ImagenUrl).HasMaxLength(500);
            builder.Property(x => x.PosterUrl).HasMaxLength(500);

            // Único solo entre las no eliminadas: si das de baja una, podés volver a cargarla.
            builder.HasIndex(x => new { x.Titulo, x.FechaEstreno })
                .IsUnique()
                .HasFilter("\"Eliminado\" = false");
        }
    }
}
