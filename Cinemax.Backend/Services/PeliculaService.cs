using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.DTO.Comun;
using Cinemax.Shared.DTO.Paginado.Request.Pelicula;
using Cinemax.Shared.DTO.Pelicula.Request;
using Cinemax.Shared.DTO.Pelicula.Response;
using Cinemax.Shared.Enums;


namespace Cinemax.Backend.Services
{
    public class PeliculaService
    {
        private readonly IPeliculaRepository _repo;
        private readonly IGeneroRepository _generoRepo;
        private readonly IPersonaRepository _personaRepo;
        private readonly ImagenMapeador _imagenes;


        public PeliculaService(IPeliculaRepository repo, IGeneroRepository generoRepo,
            IPersonaRepository personaRepo, ImagenMapeador imagenes)
        {
            _repo = repo; _generoRepo = generoRepo; _personaRepo = personaRepo; _imagenes = imagenes;
        }

        public async Task<PaginadoResponse<PeliculaListadoResponse>> ObtenerTodos(ParametroPaginacionPeliculaRequest p)
        {
            var resultado = await _repo.ObtenerTodos(ConstruirQuery(p, soloCartelera: false));
            var datos = resultado.Datos.Select(x => new PeliculaListadoResponse(
                x.Id, x.Titulo, x.Clasificacion, x.FechaEstreno, x.DuracionMinutos, x.EnCartelera)).ToList();
            return new PaginadoResponse<PeliculaListadoResponse>(datos, resultado.NumeroPagina, resultado.TamanoPagina, resultado.TotalRegistros);
        }

        public async Task<PaginadoResponse<PeliculaCatalogoResponse>> ObtenerCartelera(ParametroPaginacionPeliculaRequest p)
        {
            var resultado = await _repo.ObtenerTodos(ConstruirQuery(p, soloCartelera: true));
            var datos = resultado.Datos.Select(x => new PeliculaCatalogoResponse(
                x.Id, x.Titulo, x.Clasificacion, x.DuracionMinutos, _imagenes.Mapear(ObtenerImagen(x, TipoImagen.Poster)),
                x.Generos.Select(g => g.Genero.Nombre).ToList())).ToList();
            return new PaginadoResponse<PeliculaCatalogoResponse>(datos, resultado.NumeroPagina, resultado.TamanoPagina, resultado.TotalRegistros);
        }
        private static Imagen? ObtenerImagen(Pelicula p, TipoImagen tipo) =>
             p.Imagenes.FirstOrDefault(i => i.TipoImagen == tipo);


        public async Task<PeliculaResponse> ObtenerPorId(int id)
        {
            Pelicula pelicula = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna película con ese id.");
            return Mapear(pelicula);
        }

        public async Task<PeliculaResponse> Crear(CrearPeliculaRequest dto)
        {
            ValidarCampos(dto.Titulo, dto.Sinopsis, dto.DuracionMinutos);
            await ValidarRelaciones(dto.GenerosIds, dto.ActoresIds, dto.DirectoresIds);

            if (await _repo.ExistePorTitulo(dto.Titulo.Trim(), dto.FechaEstreno))
                throw new RecursoExistenteException("Ya existe una película con ese título y fecha de estreno.");

            Pelicula pelicula = new()
            {
                Titulo = dto.Titulo.Trim(),
                Sinopsis = dto.Sinopsis.Trim(),
                Clasificacion = dto.Clasificacion,
                FechaEstreno = dto.FechaEstreno,
                DuracionMinutos = dto.DuracionMinutos,
                EnCartelera = dto.EnCartelera,
                Generos = dto.GenerosIds.Distinct()
                .Select(i => new GeneroPelicula { GeneroId = i }).ToList(),
                Personas = dto.ActoresIds.Distinct()
                .Select(i => new PersonaPelicula { PersonaId = i, Rol = RolPelicula.Actor })
                .Concat(dto.DirectoresIds.Distinct()
                .Select(i => new PersonaPelicula { PersonaId = i, Rol = RolPelicula.Director }))
                .ToList()
            };

            await _repo.Crear(pelicula);
            return await ObtenerPorId(pelicula.Id);
        }

        public async Task Actualizar(int id, ActualizarPeliculaRequest dto)
        {
            ValidarCampos(dto.Titulo, dto.Sinopsis, dto.DuracionMinutos);
            await ValidarRelaciones(dto.GenerosIds, dto.ActoresIds, dto.DirectoresIds);

            Pelicula pelicula = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna película con ese id.");

            if (await _repo.ExistePorTitulo(dto.Titulo.Trim(), dto.FechaEstreno, excluirId: id))
                throw new RecursoExistenteException("Ya existe una película con ese título y fecha de estreno.");

            pelicula.Titulo = dto.Titulo.Trim();
            pelicula.Sinopsis = dto.Sinopsis.Trim();
            pelicula.Clasificacion = dto.Clasificacion;
            pelicula.FechaEstreno = dto.FechaEstreno;
            pelicula.DuracionMinutos = dto.DuracionMinutos;
            pelicula.EnCartelera = dto.EnCartelera;

            pelicula.Generos.Clear();
            foreach (int i in dto.GenerosIds.Distinct()) pelicula.Generos.Add(new GeneroPelicula { GeneroId = i });
            pelicula.Personas.Clear();
            SincronizarPersonas(pelicula, RolPelicula.Actor, dto.ActoresIds);
            SincronizarPersonas(pelicula, RolPelicula.Director, dto.DirectoresIds);

            await _repo.Actualizar(pelicula);
        }

        public async Task Eliminar(int id)
        {
            Pelicula pelicula = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna película con ese id.");
            if (await _repo.TieneFuncionesFuturas(id))
                throw new RecursoExistenteException("No se puede eliminar: la película tiene funciones programadas.");
            await _repo.Eliminar(pelicula);
            // Cuando exista Función: decidir si se puede dar de baja con funciones futuras (409).
        }

        // ---- privados ----
        private static void ValidarCampos(string titulo, string sinopsis, int duracion)
        {
            if (!Validaciones.EstanDatosBien(titulo)) throw new DatosLlegaronErradosException("El título es obligatorio.");
            if (!Validaciones.EstanDatosBien(sinopsis)) throw new DatosLlegaronErradosException("La sinopsis es obligatoria.");
            if (titulo.Trim().Length > 200) throw new DatosLlegaronErradosException("El título no puede superar 200 caracteres.");
            if (sinopsis.Trim().Length > 2000) throw new DatosLlegaronErradosException("La sinopsis no puede superar 2000 caracteres.");
            if (duracion <= 0) throw new DatosLlegaronErradosException("La duración debe ser mayor a cero.");
        }

        private async Task ValidarRelaciones(List<int> generos, List<int> actores, List<int> directores)
        {
            if (!await _generoRepo.ExistenTodos(generos.Distinct().ToList()))
                throw new RecursoNoExisteException("Algún género indicado no existe.");

            List<int> personas = actores.Concat(directores).Distinct().ToList();
            if (!await _personaRepo.ExistenTodos(personas))
                throw new RecursoNoExisteException("Alguna persona indicada no existe.");
        }

        private static PeliculaQueryParametros ConstruirQuery(ParametroPaginacionPeliculaRequest p, bool soloCartelera) => new()
        {
            NumeroPagina = Math.Max(p.NumeroPagina ?? 1, 1),
            TamanoPagina = Math.Clamp(p.TamanoPagina ?? 20, 1, 50),
            Buscar = p.Buscar?.Trim().ToLower(),
            GeneroId = p.GeneroId,
            EnCartelera = soloCartelera ? true : p.EnCartelera,
            OrdenarPor = p.OrdenarPor,
            Direccion = p.Direccion
        };

        private  PeliculaResponse Mapear(Pelicula x) => new(
            x.Id, x.Titulo, x.Sinopsis, x.Clasificacion, x.FechaEstreno, x.DuracionMinutos, _imagenes.Mapear(ObtenerImagen(x, TipoImagen.Principal)), _imagenes.Mapear(ObtenerImagen(x, TipoImagen.Poster)),
             x.EnCartelera,
            x.Generos.Select(g => new ReferenciaResponse(g.GeneroId, g.Genero.Nombre)).ToList(),
            x.Personas.Where(p => p.Rol == RolPelicula.Actor)
                .Select(p => new ReferenciaResponse(p.PersonaId, p.Persona.Nombre)).ToList(),
            x.Personas.Where(p => p.Rol == RolPelicula.Director)
                .Select(p => new ReferenciaResponse(p.PersonaId, p.Persona.Nombre)).ToList());

        private static void SincronizarPersonas(Pelicula pelicula, RolPelicula rol, IEnumerable<int> nuevosIds)
        {
            HashSet<int> destino = nuevosIds.ToHashSet();

            foreach (PersonaPelicula pp in pelicula.Personas
                .Where(x => x.Rol == rol && !destino.Contains(x.PersonaId)).ToList())
                pelicula.Personas.Remove(pp);

            HashSet<int> existentes = pelicula.Personas
                .Where(x => x.Rol == rol).Select(x => x.PersonaId).ToHashSet();

            foreach (int id in destino.Where(i => !existentes.Contains(i)))
                pelicula.Personas.Add(new PersonaPelicula { PersonaId = id, Rol = rol });
        }
    }
}
