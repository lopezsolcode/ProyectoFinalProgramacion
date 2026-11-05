using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.DTO.Auth.Request;
using Cinemax.Shared.DTO.Auth.Response;

namespace Cinemax.Backend.Services
{
    public class LoginService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;
        private readonly ISesionUsuarioRepository _sesionRepository;
        
        public LoginService(ITokenService tokenService, IUsuarioRepository usuarioRepository, ISesionUsuarioRepository sesionRepository)
        {
            _tokenService = tokenService;
            _usuarioRepository = usuarioRepository;
            _sesionRepository = sesionRepository;
        }


        public async Task<LoginResponse?> Login(LoginRequest loginRequest)
        {
            string username = loginRequest.Username?.Trim() ?? string.Empty;
            string password = loginRequest.Password?.Trim() ?? string.Empty;

            if (Validaciones.EstanDatosBien(username, password) == false)
            {
                throw new DatosLlegaronErradosException("Ya sea el usuario o la contraseña llegaron vacíos.");
            }
                Usuario? usuario = await _usuarioRepository.BuscarPorUsername(username);
                
                if (usuario is null || usuario.Eliminado)
                {
                    throw new RecursoNoExisteException("No existe ningún usuario con ese usuario.");
                }

                bool sonIguales = BCrypt.Net.BCrypt.EnhancedVerify(password, usuario.Password);

                if (sonIguales)
                {
                    var (token, _) = _tokenService.CrearToken(usuario);
                    var refreshToken = _tokenService.CrearRefreshToken();

                    var sesion = new SesionUsuario
                    {
                        UsuarioId = usuario.Id,
                        RefreshToken = _tokenService.CalcularHash(refreshToken),
                        Expira = DateTime.UtcNow.AddDays(30)
                    };

                    await _sesionRepository.Crear(sesion);

                    return new LoginResponse(token, refreshToken, sesion.Expira);
                }
                else
                {
                    throw new DatosLlegaronErradosException("La contraseña ingresada no coincide.");
                }
            
        }

    }
}
