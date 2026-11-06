using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class GeneroConfiguracion : IEntityTypeConfiguration<Genero>
{
    public void Configure(EntityTypeBuilder<Genero> builder)
    {
        builder.ToTable("Generos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(100);

        builder.HasIndex(x => x.Nombre)
            .IsUnique()
            .HasFilter("\"Eliminado\" = false");
    }
}
