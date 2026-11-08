using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack;
using Cinemax.Backend.Repositories;
using Cinemax.Backend.Utilidades;
using Cinemax.Shared.DTO.Combo.Request;
using Cinemax.Shared.DTO.Combo.Response;
using Cinemax.Shared.DTO.Paginado.Request.Combo;

namespace Cinemax.Backend.Services
{
    public class ComboService
    {
        private readonly IComboRepository _repo;
        private readonly ISnackRepository _snackRepo;
        private readonly ImagenMapeador _imagenes;

        public ComboService(IComboRepository repo, ISnackRepository snackRepo, ImagenMapeador imagenes)
        {
            _repo = repo; _snackRepo = snackRepo; _imagenes = imagenes;
        }

        public async Task<PaginadoResponse<ComboResponse>> ObtenerTodos(ParametroPaginacionComboRequest p)
        {
            var query = new ComboQueryParametros
            {
                NumeroPagina = Math.Max(p.NumeroPagina ?? 1, 1),
                TamanoPagina = Math.Clamp(p.TamanoPagina ?? 20, 1, 50),
                Buscar = p.Buscar?.Trim().ToLower(),
                OrdenarPor = p.OrdenarPor,
                Direccion = p.Direccion
            };

            var r = await _repo.ObtenerTodos(query);
            return new PaginadoResponse<ComboResponse>(r.Datos.Select(Mapear).ToList(),
                r.NumeroPagina, r.TamanoPagina, r.TotalRegistros);
        }

        public async Task<ComboResponse> ObtenerPorId(int id)
        {
            Combo combo = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún combo con ese id.");
            return Mapear(combo);
        }

        public async Task<ComboResponse> Crear(CrearComboRequest dto)
        {
            ValidarCampos(dto.Nombre, dto.Precio, dto.Items);
            await ValidarSnacks(dto.Items);

            string nombre = dto.Nombre.Trim();
            if (await _repo.ExistePorNombre(nombre))
                throw new RecursoExistenteException("Ya existe un combo con ese nombre.");

            Combo combo = new()
            {
                Nombre = nombre,
                Precio = dto.Precio,
                Componentes = dto.Items.Select(i => new ComboSnack { SnackId = i.SnackId, Cantidad = i.Cantidad }).ToList()
            };

            await _repo.Crear(combo);
            return await ObtenerPorId(combo.Id);
        }

        public async Task Actualizar(int id, ActualizarComboRequest dto)
        {
            ValidarCampos(dto.Nombre, dto.Precio, dto.Items);
            await ValidarSnacks(dto.Items);

            string nombre = dto.Nombre.Trim();
            Combo combo = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún combo con ese id.");

            if (await _repo.ExistePorNombre(nombre, excluirId: id))
                throw new RecursoExistenteException("Ya existe un combo con ese nombre.");

            combo.Nombre = nombre;
            combo.Precio = dto.Precio;

            // Sincronización por diferencia (misma técnica que en Película).
            Dictionary<int, int> nuevos = dto.Items.ToDictionary(i => i.SnackId, i => i.Cantidad);

            foreach (ComboSnack c in combo.Componentes.Where(c => !nuevos.ContainsKey(c.SnackId)).ToList())
                combo.Componentes.Remove(c);

            foreach (ComboSnack c in combo.Componentes)
                c.Cantidad = nuevos[c.SnackId];

            HashSet<int> existentes = combo.Componentes.Select(c => c.SnackId).ToHashSet();
            foreach (var (snackId, cantidad) in nuevos.Where(n => !existentes.Contains(n.Key)))
                combo.Componentes.Add(new ComboSnack { SnackId = snackId, Cantidad = cantidad });

            await _repo.Actualizar(combo);
        }

        public async Task Eliminar(int id)
        {
            Combo combo = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ningún combo con ese id.");
            await _repo.Eliminar(combo);
        }

        private static void ValidarCampos(string nombre, decimal precio, List<ComboItemRequest> items)
        {
            if (!Validaciones.EstanDatosBien(nombre))
                throw new DatosLlegaronErradosException("El nombre del combo es obligatorio.");
            if (nombre.Trim().Length > 100)
                throw new DatosLlegaronErradosException("El nombre no puede superar 100 caracteres.");
            if (precio <= 0)
                throw new DatosLlegaronErradosException("El precio debe ser mayor a cero.");

            if (items is null || items.Count == 0)
                throw new DatosLlegaronErradosException("El combo debe tener al menos un snack.");
            if (items.Any(i => i.Cantidad < 1 || i.Cantidad > 50))
                throw new DatosLlegaronErradosException("La cantidad de cada snack debe estar entre 1 y 50.");
            if (items.Select(i => i.SnackId).Distinct().Count() != items.Count)
                throw new DatosLlegaronErradosException("Un snack no puede repetirse dentro del combo.");
        }

        private async Task ValidarSnacks(List<ComboItemRequest> items)
        {
            if (!await _snackRepo.ExistenTodos(items.Select(i => i.SnackId).ToList()))
                throw new RecursoNoExisteException("Algún snack indicado no existe.");
        }

        private ComboResponse Mapear(Combo c) => new(
            c.Id, c.Nombre, c.Precio, _imagenes.Mapear(c.Imagen),
            c.Componentes.All(x => x.Snack.Inventario.CantidadDisponible >= x.Cantidad),
            c.Componentes.Select(x => new ComboItemResponse(x.SnackId, x.Snack.Nombre, x.Cantidad)).ToList());
    }
}
