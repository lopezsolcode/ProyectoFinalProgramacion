using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Usuario.Response;

public record ObtenerUsuarioResponse(
    int Id,
    string Username,
    string Email,
    Rol Rol);