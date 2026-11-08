using Cinemax.Shared.Enums;
using System.Text.Json.Serialization;

namespace Cinemax.Backend.Models
{
    public class Imagen
    {
        public int Id { get; set; }
        public required string NombreOriginal { get; set; }
        public required string NombreArchivo { get; set; }
        public required string RutaRelativa { get; set; }
        public required string TipoContenido { get; set; }
        public long TamanoBytes { get; set; }
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

        public TipoImagen TipoImagen { get; set; }

        public int? SnackId { get; set; }
        public int? ComboId { get; set; }
        public int? PersonaId { get; set; }
        public int? PeliculaId { get; set; }

        [JsonIgnore]
        public Snack? Snack { get; set; }

        [JsonIgnore]
        public Combo? Combo { get; set; }

        [JsonIgnore]
        public Persona? Persona { get; set; }

        [JsonIgnore]
        public Pelicula? Pelicula { get; set; }
    }
}