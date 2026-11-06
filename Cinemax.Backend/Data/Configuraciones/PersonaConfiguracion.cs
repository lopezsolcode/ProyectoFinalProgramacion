using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinemax.Backend.Data.Configuraciones
{
    public class PersonaConfiguracion : IEntityTypeConfiguration<Persona>
    {
        public void Configure(EntityTypeBuilder<Persona> builder)
        {
            builder.ToTable("Personas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
            builder.Property(x => x.ImagenUrl).HasMaxLength(500);
            builder.HasIndex(x => x.Nombre);
        }
    }

    public class PersonaPeliculaConfiguracion : IEntityTypeConfiguration<PersonaPelicula>
    {
        public void Configure(EntityTypeBuilder<PersonaPelicula> builder)
        {
            builder.ToTable("PersonasPeliculas");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Rol).HasConversion<string>().HasMaxLength(20);

            builder.HasOne(x => x.Pelicula).WithMany(x => x.Personas)
                .HasForeignKey(x => x.PeliculaId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Persona).WithMany(x => x.Peliculas)
                .HasForeignKey(x => x.PersonaId).OnDelete(DeleteBehavior.Restrict);

            // Misma persona puede repetirse en una película, pero no con el mismo rol.
            builder.HasIndex(x => new { x.PeliculaId, x.PersonaId, x.Rol }).IsUnique();
        }
    }
}
