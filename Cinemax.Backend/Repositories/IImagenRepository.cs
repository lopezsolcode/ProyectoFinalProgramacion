using Cinemax.Backend.Models;
using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Repositories
{
    public interface IImagenRepository
    {
        Task<Imagen?> ObtenerPorSnackId(int snackId);

        Task<Imagen?> ObtenerPorComboId(int comboId);

        Task<Imagen?> ObtenerPorPersonaId(int personaId);

        Task<Imagen?> ObtenerPorPeliculaIdYTipo(
            int peliculaId,
            TipoImagen tipoImagen);

        Task Crear(Imagen imagen);

        Task Actualizar(Imagen imagen);

        Task Eliminar(Imagen imagen);
    }
}
