namespace Cinemax.Backend.Repositories
{
    public interface IUnidadDeTrabajo
    {
        Task<T> Ejecutar<T>(Func<Task<T>> operacion);
        Task Auditar(
            int? usuarioId, 
            string accion, 
            string entidad, 
            int entidadId, 
            string? detalle = null);
    }
}
