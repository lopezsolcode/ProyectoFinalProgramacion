using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Funcion;
using Cinemax.Backend.Options;
using Cinemax.Backend.Repositories;
using Cinemax.Shared.DTO.Funcion.Request;
using Cinemax.Shared.DTO.Funcion.Response;
using Cinemax.Shared.DTO.Paginado.Request.Funcion;
using Cinemax.Shared.Enums;
using Microsoft.Extensions.Options;

namespace Cinemax.Backend.Services
{
    public class FuncionService
    {
        private readonly IFuncionRepository _repo;
        private readonly IPeliculaRepository _peliculaRepo;
        private readonly ISalaRepository _salaRepo;
        private readonly FuncionOptions _opciones;

        public FuncionService(IFuncionRepository repo, IPeliculaRepository peliculaRepo,
            ISalaRepository salaRepo, IOptions<FuncionOptions> opciones)
        {
            _repo = repo; _peliculaRepo = peliculaRepo; _salaRepo = salaRepo; _opciones = opciones.Value;
        }

        public async Task<PaginadoResponse<FuncionResponse>> ObtenerTodos(ParametroPaginacionFuncionRequest p)
        {
            var r = await _repo.ObtenerTodos(ConstruirQuery(p, soloFuturas: false));
            return new PaginadoResponse<FuncionResponse>(r.Datos.Select(Mapear).ToList(),
                r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<PaginadoResponse<FuncionCatalogoResponse>> ObtenerDisponibles(ParametroPaginacionFuncionRequest p)
        {
            var r = await _repo.ObtenerTodos(ConstruirQuery(p, soloFuturas: true));
            var datos = r.Datos.Select(f => new FuncionCatalogoResponse(
                f.Id, f.PeliculaId, f.Sala.Numero, f.Formato, f.Idioma, f.FechaHora, f.PrecioEntrada)).ToList();
            return new PaginadoResponse<FuncionCatalogoResponse>(datos, r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<FuncionResponse> ObtenerPorId(int id)
        {
            Funcion f = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna función con ese id.");
            return Mapear(f);
        }

        public async Task<MapaFuncionResponse> ObtenerMapa(int id)
        {
            Funcion f = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna función con ese id.");

            // Paso 4: acá se llama primero a LiberarVencidas(funcionId)
            List<Butaca> butacas = await _repo.ObtenerButacasDeSala(f.SalaId);
            HashSet<int> ocupadas = await _repo.ObtenerButacasOcupadas(id);

            List<ButacaFuncionResponse> datos = butacas
                .Select(b => new ButacaFuncionResponse(b.Id, b.Fila, b.Numero, b.Columna, b.Accesible, ocupadas.Contains(b.Id)))
                .ToList();

            return new MapaFuncionResponse(f.Id, f.PeliculaId, f.Pelicula.Titulo, f.Sala.Numero,
                f.Formato, f.Idioma, f.FechaHora, f.PrecioEntrada,
                datos.Count == 0 ? 0 : datos.Max(b => b.Columna),
                datos.Select(b => b.Fila).Distinct().OrderBy(x => x).ToList(),
                datos);
        }

        public async Task<FuncionResponse> Crear(CrearFuncionRequest dto)
        {
            DateTime inicio = FechasUtc.Exigir(dto.FechaHora);
            ValidarCampos(dto.Formato, dto.Idioma, dto.PrecioEntrada);

            if (inicio <= DateTime.UtcNow)
                throw new DatosLlegaronErradosException("La función debe programarse a futuro.");

            DateTime fin = await CalcularFin(dto.PeliculaId, dto.SalaId, inicio);

            if (await _repo.ExisteSolapamiento(dto.SalaId, inicio, fin))
                throw new RecursoExistenteException("La sala ya tiene una función que se superpone con ese horario.");

            Funcion funcion = new()
            {
                PeliculaId = dto.PeliculaId,
                SalaId = dto.SalaId,
                Formato = dto.Formato,
                Idioma = dto.Idioma,
                FechaHora = inicio,
                FechaFin = fin,
                PrecioEntrada = dto.PrecioEntrada
            };

            await _repo.Crear(funcion);
            return await ObtenerPorId(funcion.Id);
        }

        public async Task Actualizar(int id, ActualizarFuncionRequest dto)
        {
            DateTime inicio = FechasUtc.Exigir(dto.FechaHora);
            ValidarCampos(dto.Formato, dto.Idioma, dto.PrecioEntrada);

            Funcion funcion = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna función con ese id.");

            bool cambiaAgenda = funcion.PeliculaId != dto.PeliculaId || funcion.SalaId != dto.SalaId
                || funcion.FechaHora != inicio || funcion.Formato != dto.Formato || funcion.Idioma != dto.Idioma;

            if (cambiaAgenda)
            {
                if (await _repo.TieneReservasActivas(id))
                    throw new RecursoExistenteException(
                        "Hay entradas vendidas o reservadas: solo se puede modificar el precio.");

                if (funcion.FechaHora != inicio && inicio <= DateTime.UtcNow)
                    throw new DatosLlegaronErradosException("La función debe programarse a futuro.");

                DateTime fin = await CalcularFin(dto.PeliculaId, dto.SalaId, inicio);

                if (await _repo.ExisteSolapamiento(dto.SalaId, inicio, fin, excluirId: id))
                    throw new RecursoExistenteException("La sala ya tiene una función que se superpone con ese horario.");

                funcion.PeliculaId = dto.PeliculaId;
                funcion.SalaId = dto.SalaId;
                funcion.Formato = dto.Formato;
                funcion.Idioma = dto.Idioma;
                funcion.FechaHora = inicio;
                funcion.FechaFin = fin;
            }

            funcion.PrecioEntrada = dto.PrecioEntrada; // el precio de las reservas ya hechas está congelado
            await _repo.Actualizar(funcion);
        }

        public async Task Eliminar(int id)
        {
            Funcion funcion = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna función con ese id.");

            if (await _repo.TieneReservasActivas(id))
                throw new RecursoExistenteException("No se puede eliminar: hay entradas vendidas o reservadas.");

            await _repo.Eliminar(funcion);
        }

        // ---- privados ----
        private async Task<DateTime> CalcularFin(int peliculaId, int salaId, DateTime inicio)
        {
            int? duracion = await _peliculaRepo.ObtenerDuracionMinutos(peliculaId)
                ?? throw new RecursoNoExisteException("No existe ninguna película con ese id.");

            if (await _salaRepo.ObtenerPorId(salaId) is null)
                throw new RecursoNoExisteException("No existe ninguna sala con ese id.");

            return inicio.AddMinutes(duracion.Value + _opciones.MargenLimpiezaMinutos);
        }

        private static void ValidarCampos(FormatoFuncion formato, IdiomaFuncion idioma, decimal precio)
        {
            if (!Enum.IsDefined(formato)) throw new DatosLlegaronErradosException("El formato no es válido.");
            if (!Enum.IsDefined(idioma)) throw new DatosLlegaronErradosException("El idioma no es válido.");
            if (precio <= 0) throw new DatosLlegaronErradosException("El precio de la entrada debe ser mayor a cero.");
        }

        private static FuncionQueryParametros ConstruirQuery(ParametroPaginacionFuncionRequest p, bool soloFuturas) => new()
        {
            NumeroPagina = Math.Max(p.NumeroPagina ?? 1, 1),
            TamanoPagina = Math.Clamp(p.TamanoPagina ?? 20, 1, 50),
            PeliculaId = p.PeliculaId,
            SalaId = p.SalaId,
            Desde = FechasUtc.Normalizar(p.Desde),
            Hasta = FechasUtc.Normalizar(p.Hasta),
            SoloFuturas = soloFuturas,
            OrdenarPor = p.OrdenarPor,
            Direccion = p.Direccion
        };

        private static FuncionResponse Mapear(Funcion f) => new(
            f.Id, f.PeliculaId, f.Pelicula.Titulo, f.SalaId, f.Sala.Numero,
            f.Formato, f.Idioma, f.FechaHora, f.FechaFin, f.PrecioEntrada);
    }
}
