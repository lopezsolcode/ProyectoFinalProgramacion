using Microsoft.EntityFrameworkCore;
using Cinemax.Backend.Models;
using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Data;

public static class SeedDatos
{
    public static async Task CrearAdministrador(DataContext context)
    {
        bool existe = await context.Usuarios
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Username == "admin");

        if (existe)
            return;

        var usuario = new Usuario
        {
            Username = "admin",
            Password = BCrypt.Net.BCrypt.EnhancedHashPassword("admin"),
            Email = "admin@tp6.local",
            Rol = Rol.Administrador
        };

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();
    }
}
