using Microsoft.EntityFrameworkCore;
using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Repositories;

namespace Cinemax.Backend.Implementacion;

public class SesionUsuarioRepositoryPsqlEF : ISesionUsuarioRepository
{
    private readonly DataContext _dataContext;

    public SesionUsuarioRepositoryPsqlEF(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public async Task Crear(SesionUsuario sesion)
    {
        _dataContext.SesionesUsuarios.Add(sesion);
        await _dataContext.SaveChangesAsync();
    }

    public async Task<SesionUsuario?> ObtenerPorRefreshToken(string refreshToken)
    {
        return await _dataContext.SesionesUsuarios
            .Include(s => s.Usuario)
            .SingleOrDefaultAsync(s => s.RefreshToken == refreshToken);
    }

    public async Task GuardarCambios()
    {
        await _dataContext.SaveChangesAsync();
    }
}