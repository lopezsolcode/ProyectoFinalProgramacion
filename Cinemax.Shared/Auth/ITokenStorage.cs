namespace Cinemax.Shared.Auth;

public interface ITokenStorage
{
    Task<string?> Obtener();
    Task Guardar(string token);
    Task Eliminar();
}