using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.DTO.Paginado.Request.Snack;
using Cinemax.Shared.DTO.Snack.Request;
using Cinemax.Shared.DTO.Snack.Response;

namespace Cinemax.Backend.Services
{
    public class SnackService
    {
        private readonly ISnackRepository _repo;
        private readonly IInventarioRepository _inventarioRepo;
        private readonly ImagenMapeador _imagenes;

        public SnackService(ISnackRepository repo, IInventarioRepository inventarioRepo, ImagenMapeador imagenes)
        {
            _repo = repo; _inventarioRepo = inventarioRepo; _imagenes = imagenes;
        }

        public async Task<PaginadoResponse<SnackResponse>> ObtenerTodos(ParametroPaginacionSnackRequest p)
        {
            var r = await _repo.ObtenerTodos(ConstruirQuery(p));
            var datos = r.Datos.Select(Mapear).ToList();
            return new PaginadoResponse<SnackResponse>(datos, r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<PaginadoResponse<SnackCatalogoResponse>> ObtenerCatalogo(ParametroPaginacionSnackRequest p)
        {
            var r = await _repo.ObtenerTodos(ConstruirQuery(p));
            var datos = r.Datos.Select(s => new SnackCatalogoResponse(s.Id, s.Nombre, s.Precio, _imagenes.Mapear(s.Imagen),
    s.Inventario.DescripcionUnidad, s.Inventario.CantidadDisponible > 0)).ToList();
            return new PaginadoResponse<SnackCatalogoResponse>(datos, r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<SnackResponse> ObtenerPorId(int id)
        {
            Snack snack = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún snack con ese id.");
            return Mapear(snack);
        }

        public async Task<SnackResponse> Crear(CrearSnackRequest dto)
        {
            ValidarCampos(dto.Nombre, dto.Precio, dto.DescripcionUnidad);
            if (dto.StockInicial < 0)
                throw new DatosLlegaronErradosException("El stock inicial no puede ser negativo.");

            string nombre = dto.Nombre.Trim();
            if (await _repo.ExistePorNombre(nombre))
                throw new RecursoExistenteException("Ya existe un snack con ese nombre.");

            Snack snack = new()
            {
                Nombre = nombre,
                Precio = dto.Precio,
                Inventario = new Inventario
                {
                    DescripcionUnidad = dto.DescripcionUnidad.Trim(),
                    CantidadDisponible = dto.StockInicial
                }
            };

            await _repo.Crear(snack);
            return Mapear(snack);
        }

        public async Task Actualizar(int id, ActualizarSnackRequest dto)
        {
            ValidarCampos(dto.Nombre, dto.Precio, dto.DescripcionUnidad);
            string nombre = dto.Nombre.Trim();

            Snack snack = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún snack con ese id.");

            if (await _repo.ExistePorNombre(nombre, excluirId: id))
                throw new RecursoExistenteException("Ya existe un snack con ese nombre.");

            snack.Nombre = nombre;
            snack.Precio = dto.Precio; // las ventas ya hechas tienen su precio congelado
            snack.Inventario.DescripcionUnidad = dto.DescripcionUnidad.Trim();

            await _repo.Actualizar(snack);
        }

        public async Task<SnackResponse> AjustarStock(int id, AjustarStockRequest dto)
        {
            if (dto.Delta == 0)
                throw new DatosLlegaronErradosException("El ajuste no puede ser cero.");

            // Existe() no trackea: así la lectura posterior ve el valor actualizado por el UPDATE.
            if (!await _repo.Existe(id))
                throw new RecursoNoExisteException("No existe ningún snack con ese id.");

            if (!await _inventarioRepo.TryAjustar(id, dto.Delta))
                throw new StockInsuficienteException("El ajuste dejaría el stock en negativo.");

            return await ObtenerPorId(id);
        }

        public async Task Eliminar(int id)
        {
            Snack snack = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún snack con ese id.");

            if (await _repo.EstaEnCombosActivos(id))
                throw new RecursoExistenteException("No se puede eliminar: el snack forma parte de un combo.");

            await _repo.Eliminar(snack);
        }

        private static void ValidarCampos(string nombre, decimal precio, string unidad)
        {
            if (!Validaciones.EstanDatosBien(nombre))
                throw new DatosLlegaronErradosException("El nombre del snack es obligatorio.");
            if (nombre.Trim().Length > 100)
                throw new DatosLlegaronErradosException("El nombre no puede superar 100 caracteres.");
            if (precio <= 0)
                throw new DatosLlegaronErradosException("El precio debe ser mayor a cero.");
            if (!Validaciones.EstanDatosBien(unidad) || unidad.Trim().Length > 50)
                throw new DatosLlegaronErradosException("La descripción de la unidad es obligatoria (máx. 50 caracteres).");
        }


        private static SnackQueryParametros ConstruirQuery(ParametroPaginacionSnackRequest p) => new()
        {
            NumeroPagina = Math.Max(p.NumeroPagina ?? 1, 1),
            TamanoPagina = Math.Clamp(p.TamanoPagina ?? 20, 1, 50),
            Buscar = p.Buscar?.Trim().ToLower(),
            SoloDisponibles = p.SoloDisponibles,
            OrdenarPor = p.OrdenarPor,
            Direccion = p.Direccion
        };

        private SnackResponse Mapear(Snack s) => new(
            s.Id, s.Nombre, s.Precio, _imagenes.Mapear(s.Imagen),
            s.Inventario.DescripcionUnidad, s.Inventario.CantidadDisponible);
            }
}
