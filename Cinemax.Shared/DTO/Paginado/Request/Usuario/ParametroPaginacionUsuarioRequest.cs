using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Paginado.Request.Usuario;

public record ParametroPaginacionUsuarioRequest : ParametroPaginacionRequest
{
    public bool? Eliminado { get; init; }
    public Rol? Rol { get; init; }
};