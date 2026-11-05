using System;
using System.Collections.Generic;
using System.Text;
using Cinemax.Shared.Enums;

namespace Cinemax.Shared.DTO.Auth.Request
{
    public record CrearEmpleadoRequest(
        string Username,
        string Password,
        string Email,
        Rol Rol);
}
