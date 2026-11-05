using Cinemax.Shared.Enums;

namespace Cinemax.Shared.Auth;

public record UsuarioSesion(int Id, string Username, string Email, Rol Rol);