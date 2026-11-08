using Cinemax.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cinemax.Backend.Data.Configuraciones
{
    public class ImagenConfiguracion : IEntityTypeConfiguration<Imagen>
    {
        public void Configure(EntityTypeBuilder<Imagen> builder)
        {
            builder.ToTable("Imagenes", table =>
            {
                table.HasCheckConstraint(
                    "CK_Imagen_UnaSolaEntidad",
                    """
                    (
                        (
                            "SnackId" IS NOT NULL
                            AND "ComboId" IS NULL
                            AND "PersonaId" IS NULL
                            AND "PeliculaId" IS NULL
                            AND "TipoImagen" = 'Unica'
                        )
                        OR
                        (
                            "SnackId" IS NULL
                            AND "ComboId" IS NOT NULL
                            AND "PersonaId" IS NULL
                            AND "PeliculaId" IS NULL
                            AND "TipoImagen" = 'Unica'
                        )
                        OR
                        (
                            "SnackId" IS NULL
                            AND "ComboId" IS NULL
                            AND "PersonaId" IS NOT NULL
                            AND "PeliculaId" IS NULL
                            AND "TipoImagen" = 'Unica'
                        )
                        OR
                        (
                            "SnackId" IS NULL
                            AND "ComboId" IS NULL
                            AND "PersonaId" IS NULL
                            AND "PeliculaId" IS NOT NULL
                            AND "TipoImagen" IN ('Principal', 'Poster')
                        )
                    )
                    """
                );
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.NombreOriginal).IsRequired();
            builder.Property(x => x.NombreArchivo).IsRequired();
            builder.Property(x => x.RutaRelativa).IsRequired();
            builder.Property(x => x.TipoContenido).IsRequired();
            builder.Property(x => x.TamanoBytes).IsRequired();
            builder.Property(x => x.FechaCreacion).IsRequired();

            builder.Property(x => x.TipoImagen)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            // Snack: una imagen como máximo.
            builder.HasOne(x => x.Snack)
                .WithOne(x => x.Imagen)
                .HasForeignKey<Imagen>(x => x.SnackId)
                .OnDelete(DeleteBehavior.Cascade);

            // Combo: una imagen como máximo.
            builder.HasOne(x => x.Combo)
                .WithOne(x => x.Imagen)
                .HasForeignKey<Imagen>(x => x.ComboId)
                .OnDelete(DeleteBehavior.Cascade);

            // Persona: una imagen como máximo.
            builder.HasOne(x => x.Persona)
                .WithOne(x => x.Imagen)
                .HasForeignKey<Imagen>(x => x.PersonaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Película: varias imágenes, diferenciadas por TipoImagen.
            builder.HasOne(x => x.Pelicula)
                .WithMany(x => x.Imagenes)
                .HasForeignKey(x => x.PeliculaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.SnackId).IsUnique();
            builder.HasIndex(x => x.ComboId).IsUnique();
            builder.HasIndex(x => x.PersonaId).IsUnique();

            // Una sola imagen principal y un solo póster por película.
            builder.HasIndex(x => new { x.PeliculaId, x.TipoImagen })
                .IsUnique()
                .HasFilter("\"PeliculaId\" IS NOT NULL");
        }
    }
}

