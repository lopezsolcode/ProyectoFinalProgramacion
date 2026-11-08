using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Sala;
using Cinemax.Backend.Options;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.DTO.Paginado.Request.Sala;
using Cinemax.Shared.DTO.Sala.Request;
using Cinemax.Shared.DTO.Sala.Response;
using Microsoft.Extensions.Options;

namespace Cinemax.Backend.Services
{
    public class SalaService
    {
        private readonly ISalaRepository _repo;
        private readonly SalaLayoutOptions _layout;

        public SalaService(ISalaRepository repo, IOptions<SalaLayoutOptions> layout)
        {
            _repo = repo;
            _layout = layout.Value;
        }

        public async Task<PaginadoResponse<SalaResponse>> ObtenerTodos(ParametroPaginacionSalaRequest p)
        {
            var query = new SalaQueryParametros
            {
                NumeroPagina = Math.Max(p.NumeroPagina ?? 1, 1),
                TamanoPagina = Math.Clamp(p.TamanoPagina ?? 20, 1, 50),
                OrdenarPor = p.OrdenarPor,
                Direccion = p.Direccion
            };

            var r = await _repo.ObtenerTodos(query);
            var datos = r.Datos.Select(s => new SalaResponse(s.Id, s.Numero, s.CantidadButacas)).ToList();
            return new PaginadoResponse<SalaResponse>(datos, r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<SalaResponse> ObtenerPorId(int id)
        {
            Sala sala = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna sala con ese id.");
            return new SalaResponse(sala.Id, sala.Numero, await _repo.ContarButacas(id));
        }

        public async Task<MapaSalaResponse> ObtenerMapa(int id)
        {
            Sala sala = await _repo.ObtenerConButacas(id)
                ?? throw new RecursoNoExisteException("No existe ninguna sala con ese id.");

            List<ButacaResponse> butacas = sala.Butacas
                .Select(b => new ButacaResponse(b.Id, b.Fila, b.Numero, b.Columna, b.Accesible))
                .ToList();

            return new MapaSalaResponse(
                sala.Id,
                sala.Numero,
                butacas.Count == 0 ? 0 : butacas.Max(b => b.Columna),
                butacas.Select(b => b.Fila).Distinct().OrderBy(f => f).ToList(),
                butacas);
        }

        public async Task<SalaResponse> Crear(CrearSalaRequest dto)
        {
            ValidarNumero(dto.Numero);

            if (await _repo.ExistePorNumero(dto.Numero))
                throw new RecursoExistenteException("Ya existe una sala con ese número.");

            Sala sala = new()
            {
                Numero = dto.Numero,
                Butacas = GeneradorButacas.Generar(_layout)
            };

            await _repo.Crear(sala);
            return new SalaResponse(sala.Id, sala.Numero, sala.Butacas.Count);
        }

        public async Task Actualizar(int id, ActualizarSalaRequest dto)
        {
            ValidarNumero(dto.Numero);

            Sala sala = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna sala con ese id.");

            if (await _repo.ExistePorNumero(dto.Numero, excluirId: id))
                throw new RecursoExistenteException("Ya existe una sala con ese número.");

            sala.Numero = dto.Numero;
            await _repo.Actualizar(sala);
        }

        public async Task Eliminar(int id)
        {
            Sala sala = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna sala con ese id.");

            if (await _repo.TieneFuncionesFuturas(id))
                throw new RecursoExistenteException("No se puede eliminar: la sala tiene funciones programadas.");

            await _repo.Eliminar(sala);
        }

        private static void ValidarNumero(int numero)
        {
            if (numero <= 0)
                throw new DatosLlegaronErradosException("El número de sala debe ser mayor a cero.");
        }
    }
}
