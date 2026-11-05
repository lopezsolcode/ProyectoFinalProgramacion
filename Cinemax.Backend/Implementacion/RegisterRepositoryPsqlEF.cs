using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Repositories;

namespace Cinemax.Backend.Implementacion
{
    public class RegisterRepositoryPsqlEF : IRegisterRepository
    {
        private readonly DataContext _dataContext;
        public RegisterRepositoryPsqlEF(DataContext dataContext)
        {
            _dataContext = dataContext;
        }
        
        public async Task Registrarse (Usuario usuario)
        {
            _dataContext.Usuarios.Add(usuario);

            await _dataContext.SaveChangesAsync();
        }
    }
}
