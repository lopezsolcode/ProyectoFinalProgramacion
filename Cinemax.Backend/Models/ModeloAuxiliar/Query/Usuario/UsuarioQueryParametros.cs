using Cinemax.Shared.Enums;
namespace Cinemax.Backend.Models.ModeloAuxiliar.Query.Usuario;

public record UsuarioQueryParametros : QueryParametros
{
    public bool? Eliminado { get; init; }
    public Rol? Rol { get; init; }
}