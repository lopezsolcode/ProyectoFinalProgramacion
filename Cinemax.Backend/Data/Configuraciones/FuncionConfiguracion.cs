// Data/Configuraciones/FuncionConfiguracion.cs
using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class FuncionConfiguracion : IEntityTypeConfiguration<Funcion>
{
    public void Configure(EntityTypeBuilder<Funcion> builder)
    {
        builder.ToTable("Funciones");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Formato).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Idioma).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.PrecioEntrada).HasPrecision(12, 2);

        builder.HasOne(x => x.Sala).WithMany(x => x.Funciones)
            .HasForeignKey(x => x.SalaId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Pelicula).WithMany(x => x.Funciones)
            .HasForeignKey(x => x.PeliculaId).OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SalaId, x.FechaHora });
        builder.HasIndex(x => new { x.PeliculaId, x.FechaHora });
    }
}