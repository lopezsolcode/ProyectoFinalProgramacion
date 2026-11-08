using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Repositories;
using Cinemax.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    public class ImagenRepositoryPsqlEF : IImagenRepository
    {
        private readonly DataContext _dataContext;

        public ImagenRepositoryPsqlEF(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<Imagen?> ObtenerPorSnackId(int snackId)
        {
            return await _dataContext.Imagenes
                .FirstOrDefaultAsync(i => i.SnackId == snackId);
        }

        public async Task<Imagen?> ObtenerPorComboId(int comboId)
        {
            return await _dataContext.Imagenes
                .FirstOrDefaultAsync(i => i.ComboId == comboId);
        }

        public async Task<Imagen?> ObtenerPorPersonaId(int personaId)
        {
            return await _dataContext.Imagenes
                .FirstOrDefaultAsync(i => i.PersonaId == personaId);
        }

        public async Task<Imagen?> ObtenerPorPeliculaIdYTipo(
            int peliculaId,
            TipoImagen tipoImagen)
        {
            return await _dataContext.Imagenes
                .FirstOrDefaultAsync(i =>
                    i.PeliculaId == peliculaId &&
                    i.TipoImagen == tipoImagen);
        }

        public async Task Crear(Imagen imagen)
        {
            _dataContext.Imagenes.Add(imagen);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Actualizar(Imagen imagen)
        {
            _dataContext.Imagenes.Update(imagen);
            await _dataContext.SaveChangesAsync();
        }

        public async Task Eliminar(Imagen imagen)
        {
            _dataContext.Imagenes.Remove(imagen);
            await _dataContext.SaveChangesAsync();
        }
    }
}
