namespace Cinemax.Shared.DTO.Auth.Request;

public record LoginRequest(
    string Username,
    string Password);