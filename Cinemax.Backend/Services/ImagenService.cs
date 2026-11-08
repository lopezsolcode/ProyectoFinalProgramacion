using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Repositories;
using Cinemax.Shared.Enums;

namespace Cinemax.Backend.Services
{
    public class ImagenService
    {
        private readonly IImagenRepository _imagenRepo;
        private readonly ISnackRepository _snackRepo;
        private readonly IComboRepository _comboRepo;
        private readonly IPersonaRepository _personaRepo;
        private readonly IPeliculaRepository _peliculaRepo;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] _extensionesPermitidas =
            [".jpg", ".jpeg", ".png"];

        private static readonly string[] _tiposMimePermitidos =
            ["image/jpeg", "image/png"];

        private const long MaxFileSize = 5 * 1024 * 1024;

        public ImagenService(
            IImagenRepository imagenRepo,
            ISnackRepository snackRepo,
            IComboRepository comboRepo,
            IPersonaRepository personaRepo,
            IPeliculaRepository peliculaRepo,
            IWebHostEnvironment env)
        {
            _imagenRepo = imagenRepo;
            _snackRepo = snackRepo;
            _comboRepo = comboRepo;
            _personaRepo = personaRepo;
            _peliculaRepo = peliculaRepo;
            _env = env;
        }

        // SNACK

        public async Task<Imagen> SubirImagenSnack(
            int snackId,
            IFormFile archivo)
        {
            if (!await _snackRepo.Existe(snackId))
                throw new RecursoNoExisteException(
                    "No existe ningún snack con ese id.");

            return await GuardarImagen(
                archivo,
                TipoImagen.Unica,
                snackId: snackId);
        }

        // COMBO

        public async Task<Imagen> SubirImagenCombo(
            int comboId,
            IFormFile archivo)
        {
            if (!await _comboRepo.Existe(comboId))
                throw new RecursoNoExisteException(
                    "No existe ningún combo con ese id.");

            return await GuardarImagen(
                archivo,
                TipoImagen.Unica,
                comboId: comboId);
        }

        // PERSONA

        public async Task<Imagen> SubirImagenPersona(
            int personaId,
            IFormFile archivo)
        {
            if (await _personaRepo.ObtenerPorId(personaId) is null)
                throw new RecursoNoExisteException(
                    "No existe ninguna persona con ese id.");

            return await GuardarImagen(
                archivo,
                TipoImagen.Unica,
                personaId: personaId);
        }

        // PELÍCULA: IMAGEN PRINCIPAL

        public async Task<Imagen> SubirImagenPrincipalPelicula(
            int peliculaId,
            IFormFile archivo)
        {
            if (await _peliculaRepo.ObtenerPorId(peliculaId) is null)
                throw new RecursoNoExisteException(
                    "No existe ninguna película con ese id.");

            return await GuardarImagen(
                archivo,
                TipoImagen.Principal,
                peliculaId: peliculaId);
        }

        // PELÍCULA: POSTER

        public async Task<Imagen> SubirPosterPelicula(
            int peliculaId,
            IFormFile archivo)
        {
            if (await _peliculaRepo.ObtenerPorId(peliculaId) is null)
                throw new RecursoNoExisteException(
                    "No existe ninguna película con ese id.");

            return await GuardarImagen(
                archivo,
                TipoImagen.Poster,
                peliculaId: peliculaId);
        }

        // GUARDADO Y REEMPLAZO

        private async Task<Imagen> GuardarImagen(
            IFormFile archivo,
            TipoImagen tipoImagen,
            int? snackId = null,
            int? comboId = null,
            int? personaId = null,
            int? peliculaId = null)
        {
            ValidarArchivo(archivo);

            string extension = Path.GetExtension(archivo.FileName)
                .ToLowerInvariant();

            string nombreUnico = $"{Guid.NewGuid()}{extension}";

            string webRoot = ObtenerWebRoot();
            string uploadsDir = Path.Combine(webRoot, "uploads");

            Directory.CreateDirectory(uploadsDir);

            string rutaFisicaNueva = Path.Combine(
                uploadsDir,
                nombreUnico);

            string rutaRelativaNueva = $"uploads/{nombreUnico}";

            // Buscar si ya existe una imagen para esa entidad y tipo.
            Imagen? imagenExistente = await ObtenerImagenExistente(
                snackId,
                comboId,
                personaId,
                peliculaId,
                tipoImagen);

            string? rutaAnterior = imagenExistente?.RutaRelativa;

            // Guardar primero el archivo nuevo.
            try
            {
                await using var stream = new FileStream(
                    rutaFisicaNueva,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None);

                await archivo.CopyToAsync(stream);
            }
            catch
            {
                EliminarArchivoFisico(rutaRelativaNueva);
                throw;
            }

            if (imagenExistente is not null)
            {
                // Actualizar el registro existente.
                imagenExistente.NombreOriginal =
                    Path.GetFileName(archivo.FileName);

                imagenExistente.NombreArchivo = nombreUnico;
                imagenExistente.RutaRelativa = rutaRelativaNueva;
                imagenExistente.TipoContenido = archivo.ContentType;
                imagenExistente.TamanoBytes = archivo.Length;
                imagenExistente.FechaCreacion = DateTime.UtcNow;

                try
                {
                    await _imagenRepo.Actualizar(imagenExistente);
                }
                catch
                {
                    // Si falla la base de datos, conservar el archivo anterior.
                    EliminarArchivoFisico(rutaRelativaNueva);
                    throw;
                }

                // El registro ya apunta al archivo nuevo.
                // Ahora podemos eliminar el archivo anterior.
                if (!string.IsNullOrWhiteSpace(rutaAnterior))
                    EliminarArchivoFisico(rutaAnterior);

                return imagenExistente;
            }

            // Si no había imagen, crear un registro nuevo.
            Imagen imagen = new()
            {
                NombreOriginal = Path.GetFileName(archivo.FileName),
                NombreArchivo = nombreUnico,
                RutaRelativa = rutaRelativaNueva,
                TipoContenido = archivo.ContentType,
                TamanoBytes = archivo.Length,
                FechaCreacion = DateTime.UtcNow,
                TipoImagen = tipoImagen,
                SnackId = snackId,
                ComboId = comboId,
                PersonaId = personaId,
                PeliculaId = peliculaId
            };

            try
            {
                await _imagenRepo.Crear(imagen);
            }
            catch
            {
                EliminarArchivoFisico(rutaRelativaNueva);
                throw;
            }

            return imagen;
        }

        // CONSULTA DE IMAGEN EXISTENTE

        private async Task<Imagen?> ObtenerImagenExistente(
            int? snackId,
            int? comboId,
            int? personaId,
            int? peliculaId,
            TipoImagen tipoImagen)
        {
            if (snackId.HasValue)
                return await _imagenRepo.ObtenerPorSnackId(snackId.Value);

            if (comboId.HasValue)
                return await _imagenRepo.ObtenerPorComboId(comboId.Value);

            if (personaId.HasValue)
                return await _imagenRepo.ObtenerPorPersonaId(personaId.Value);

            if (peliculaId.HasValue)
                return await _imagenRepo.ObtenerPorPeliculaIdYTipo(
                    peliculaId.Value,
                    tipoImagen);

            throw new InvalidOperationException(
                "No se especificó una entidad para asociar la imagen.");
        }

        // VALIDACIONES

        private static void ValidarArchivo(IFormFile archivo)
        {
            if (archivo is null || archivo.Length == 0)
                throw new DatosLlegaronErradosException(
                    "Debe proporcionar un archivo de imagen válido.");

            if (archivo.Length > MaxFileSize)
                throw new DatosLlegaronErradosException(
                    "El archivo excede el tamaño máximo permitido de 5MB.");

            string extension = Path.GetExtension(archivo.FileName)
                .ToLowerInvariant();

            if (!_extensionesPermitidas.Contains(extension))
                throw new DatosLlegaronErradosException(
                    "Formato no permitido. Solo se aceptan archivos PNG y JPG/JPEG.");

            if (string.IsNullOrWhiteSpace(archivo.ContentType) ||
                !_tiposMimePermitidos.Contains(
                    archivo.ContentType.ToLowerInvariant()))
            {
                throw new DatosLlegaronErradosException(
                    "Tipo MIME inválido para la imagen.");
            }
        }

        // ARCHIVOS FÍSICOS

        private string ObtenerWebRoot()
        {
            return _env.WebRootPath
                ?? Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot");
        }

        private void EliminarArchivoFisico(string rutaRelativa)
        {
            string uploadsDir = Path.GetFullPath(
                Path.Combine(ObtenerWebRoot(), "uploads"));

            string rutaFisica = Path.GetFullPath(
                Path.Combine(ObtenerWebRoot(), rutaRelativa));

            // Evitar eliminar archivos fuera de wwwroot/uploads.
            string prefijoPermitido = uploadsDir + Path.DirectorySeparatorChar;

            if (!rutaFisica.StartsWith(
                prefijoPermitido,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (File.Exists(rutaFisica))
                File.Delete(rutaFisica);
        }
    }
}
