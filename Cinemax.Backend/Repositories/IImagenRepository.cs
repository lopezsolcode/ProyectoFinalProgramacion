using Cinemax.Backend.Models;

namespace Cinemax.Backend.Repositories;

public interface IImagenRepository
{
    Task<Imagen?> ObtenerPorProductoId(int productoId);
    Task Crear(Imagen imagen);
    Task Eliminar(Imagen imagen);
}
