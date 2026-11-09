using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Options;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Snack;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Venta;
using Cinemax.Backend.PasarelaSimulada.Pagos;
using Cinemax.Backend.Repositories;
using Cinemax.Shared.DTO.Paginado.Request.Venta;
using Cinemax.Shared.DTO.PDFs.Response;
using Cinemax.Shared.DTO.Venta.Request;
using Cinemax.Shared.DTO.Venta.Response;
using Cinemax.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace Cinemax.Backend.Services
{
    public class VentaService
    {
        private readonly IVentaRepository _repo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IFuncionRepository _funcionRepo;
        private readonly ISnackRepository _snackRepo;
        private readonly IComboRepository _comboRepo;
        private readonly InventarioService _inventario;
        private readonly IUnidadDeTrabajo _uow;
        private readonly VentaOptions _opciones;

        public VentaService(
            IVentaRepository repo,
            IClienteRepository clienteRepo,
            IFuncionRepository funcionRepo,
            ISnackRepository snackRepo,
            IComboRepository comboRepo,
            InventarioService inventario,
            IUnidadDeTrabajo uow,
            IOptions<VentaOptions> opciones)
        {
            _repo = repo;
            _clienteRepo = clienteRepo;
            _funcionRepo = funcionRepo;
            _snackRepo = snackRepo;
            _comboRepo = comboRepo;
            _inventario = inventario;
            _uow = uow;
            _opciones = opciones.Value;
        }
        public async Task<PaginadoResponse<VentaListadoResponse>> ObtenerTodas(
           ParametroPaginacionVentaRequest parametros, int? clienteIdForzado = null)
        {
            try
            {
                int numeroPagina = parametros.NumeroPagina is null || parametros.NumeroPagina < 1
                    ? 1
                    : parametros.NumeroPagina.Value;

                int tamanoPagina = parametros.TamanoPagina is null || parametros.TamanoPagina < 1
                    ? 20
                    : parametros.TamanoPagina > 50 ? 50 : parametros.TamanoPagina.Value;

                int? clienteId = clienteIdForzado ?? parametros.ClienteId;
                EstadoVenta? estado = parametros.Estado;

                string? direccion =
                    string.IsNullOrWhiteSpace(parametros.Direccion) ||
                    parametros.Direccion?.ToLower() is not ("asc" or "desc")
                        ? "desc"
                        : parametros.Direccion;

                string? buscar = parametros.Buscar?.Trim().ToLower();

                string? ordenarPor = string.IsNullOrWhiteSpace(parametros.OrdenarPor) ? "fecha" : parametros.OrdenarPor;

                VentaQueryParametros ventaQueryParametros = new VentaQueryParametros
                {
                    NumeroPagina = numeroPagina,
                    TamanoPagina = tamanoPagina,
                    ClienteId = clienteId,
                    Estado = estado,
                    Buscar = buscar,
                    OrdenarPor = ordenarPor,
                    Direccion = direccion
                };

                PaginadoResponse<Venta> resultado = await _repo.ObtenerTodas(ventaQueryParametros);

                List<VentaListadoResponse> ventas = resultado.Datos.Select(v => new VentaListadoResponse(
                    v.Id, v.Fecha, v.Total, v.ClienteId, v.Cliente?.Nombre, v.Estado
                )).ToList();

                return new PaginadoResponse<VentaListadoResponse>(
                    ventas,
                    resultado.NumeroPagina,
                    resultado.TamanoPagina,
                    resultado.TotalRegistros
                );
            }
            catch (DbException e)
            {
                throw new BaseDeDatosException($"Ocurrió un problema: {e.Message}");
            }
        }
        public async Task<PaginadoResponse<VentaListadoResponse>> ObtenerMisCompras(
            int usuarioId, ParametroPaginacionVentaRequest parametros)
        {
            Cliente? cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId);

            if (cliente is null)
                return new PaginadoResponse<VentaListadoResponse>(new List<VentaListadoResponse>(), 1, 20, 0);

            return await ObtenerTodas(parametros, cliente.Id);
        }
        public async Task<VentaResponse> ObtenerMiCompra(int id, int usuarioId)
        {
            Cliente? cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId);
            Venta? venta = await _repo.ObtenerPorId(id);

            if (venta is null || cliente is null || venta.ClienteId != cliente.Id)
                throw new RecursoNoExisteException("No existe ninguna compra con ese id.");

            return MapearADto(venta);
        }
        public async Task<VentaResponse> ObtenerPorId(int id)
        {
            try
            {
                Venta? venta = await _repo.ObtenerPorId(id);

                if (venta is null)
                {
                    throw new RecursoNoExisteException("No existe ninguna venta con ese id.");
                }

                return MapearADto(venta);
            }
            catch (DbException e)
            {
                throw new BaseDeDatosException($"Ocurrió un problema: {e.Message}");
            }
        }
        public async Task<VentaResponse> CrearVentaPresencial(CrearVentaPresencialRequest dto, int usuarioId)
        {
            Cliente? cliente = null;
            if (dto.ClienteId.HasValue)
            {
                cliente = await _clienteRepo.ObtenerPorId(dto.ClienteId.Value)
                    ?? throw new RecursoNoExisteException("No existe ningún cliente con ese id.");
            }

            if (!Enum.IsDefined(dto.Metodo))
                throw new DatosLlegaronErradosException("El método de pago no es válido.");

            await LiberarVencidas();

            int ventaId = await _uow.Ejecutar(async () =>
            {
                Venta venta = await ArmarVentaYReservar(dto.FuncionId, dto.ButacasIds, dto.Items, permitirSoloSnacks: true);

                venta.Estado = EstadoVenta.Confirmada;
                venta.UsuarioId = usuarioId;
                venta.ClienteId = cliente?.Id;
                venta.Pagos.Add(new Pago
                {
                    Metodo = dto.Metodo,
                    Monto = venta.Total,
                    Estado = EstadoPago.Aprobado,
                    Fecha = DateTime.UtcNow,
                    ReferenciaPago = dto.ReferenciaPago
                });

                await _repo.Crear(venta);
                await _uow.Auditar(usuarioId, "VentaPresencialCreada", nameof(Venta), venta.Id, $"Total {venta.Total}");
                return venta.Id;
            });

            return await ObtenerPorId(ventaId);
        }

        public async Task<VentaResponse> CrearCompraOnline(CrearCompraOnlineRequest dto, int usuarioId)
        {
            Cliente cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId)
                ?? throw new RecursoNoExisteException("El usuario autenticado no tiene un cliente asociado.");

            await LiberarVencidas();

            // Evita que una persona acapare butacas abriendo compras pendientes en serie.
            if (await _repo.TienePendiente(cliente.Id, DateTime.UtcNow))
                throw new RecursoExistenteException("Ya tenés una compra pendiente de pago. Pagala o cancelala antes de iniciar otra.");

            int ventaId = await _uow.Ejecutar(async () =>
            {
                Venta venta = await ArmarVentaYReservar(dto.FuncionId, dto.ButacasIds, dto.Items, permitirSoloSnacks: false);

                venta.Estado = EstadoVenta.Pendiente;
                venta.ClienteId = cliente.Id;
                venta.UsuarioId = null;
                venta.FechaExpiracion = venta.Fecha.AddMinutes(_opciones.MinutosVencimiento);

                await _repo.Crear(venta);
                await _uow.Auditar(usuarioId, "CompraOnlineCreada", nameof(Venta), venta.Id, $"Total {venta.Total}");
                return venta.Id;
            });

            return await ObtenerMiCompra(ventaId, usuarioId);
        }
        private async Task<(List<DetalleVentaSnack> Detalles, decimal Total, Dictionary<int, int> Consumo)>
            PrepararSnacks(List<VentaSnackItemRequest> items)
        {
            // Validar antes de agrupar: si no, +60 y -30 sumarían 30 y pasarían.
            if (items.Any(i => i.Cantidad < 1 || i.Cantidad > 50))
                throw new DatosLlegaronErradosException("La cantidad de cada ítem debe estar entre 1 y 50.");

            List<ItemSnackVenta> normalizados = items
                .GroupBy(i => (i.SnackId, i.ComboId))
                .Select(g => new ItemSnackVenta(g.Key.SnackId, g.Key.ComboId, g.Sum(i => i.Cantidad)))
                .ToList();

            Dictionary<int, int> consumo = await _inventario.CalcularConsumo(normalizados);

            List<int> snackIds = normalizados.Where(i => i.SnackId.HasValue).Select(i => i.SnackId!.Value).ToList();
            List<int> comboIds = normalizados.Where(i => i.ComboId.HasValue).Select(i => i.ComboId!.Value).ToList();

            // Estas consultas filtran eliminados: un snack dado de baja no se puede vender.
            Dictionary<int, Snack> snacks = (await _snackRepo.ObtenerPorIds(snackIds)).ToDictionary(s => s.Id);
            Dictionary<int, Combo> combos = (await _comboRepo.ObtenerPorIdsConComponentes(comboIds)).ToDictionary(c => c.Id);

            List<DetalleVentaSnack> detalles = new();
            decimal total = 0;

            foreach (ItemSnackVenta item in normalizados)
            {
                decimal precio;
                if (item.SnackId.HasValue)
                {
                    if (!snacks.TryGetValue(item.SnackId.Value, out Snack? s))
                        throw new RecursoNoExisteException("Algún snack indicado no existe.");
                    precio = s.Precio;
                }
                else
                {
                    precio = combos[item.ComboId!.Value].Precio; // CalcularConsumo ya verificó que existe
                }

                total += precio * item.Cantidad;
                detalles.Add(new DetalleVentaSnack
                {
                    SnackId = item.SnackId,
                    ComboId = item.ComboId,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = precio
                });
            }

            return (detalles, total, consumo);
        }

        private async Task<Venta> ArmarVentaYReservar(int? funcionId, List<int>? butacasIds, List<VentaSnackItemRequest>? items, bool permitirSoloSnacks)
        {
            butacasIds ??= new();
            items ??= new();
            bool hayEntradas = butacasIds.Count > 0;
            bool haySnacks = items.Count > 0;

            if (!hayEntradas && !haySnacks)
                throw new DatosLlegaronErradosException("La venta debe tener al menos una entrada o un snack.");
            if (!hayEntradas && !permitirSoloSnacks)
                throw new DatosLlegaronErradosException("La compra online debe incluir al menos una entrada.");
            if (hayEntradas && funcionId is null)
                throw new DatosLlegaronErradosException("Falta indicar la función.");
            if (!hayEntradas && funcionId is not null)
                throw new DatosLlegaronErradosException("Indicaste una función pero ninguna butaca.");
            if (butacasIds.Count > _opciones.MaxButacasPorVenta)
                throw new DatosLlegaronErradosException($"No se pueden comprar más de {_opciones.MaxButacasPorVenta} entradas por vez.");
            if (butacasIds.Distinct().Count() != butacasIds.Count)
                throw new DatosLlegaronErradosException("Hay butacas repetidas.");

            Venta venta = new() { Fecha = DateTime.UtcNow };
            decimal total = 0;

            if (hayEntradas)
            {
                Funcion funcion = await _funcionRepo.ObtenerPorId(funcionId!.Value)
                    ?? throw new RecursoNoExisteException("No existe ninguna función con ese id.");

                if (funcion.FechaHora <= DateTime.UtcNow)
                    throw new DatosLlegaronErradosException("La función ya comenzó.");

                List<Butaca> butacasSala = await _funcionRepo.ObtenerButacasDeSala(funcion.SalaId);
                HashSet<int> idsSala = butacasSala.Select(b => b.Id).ToHashSet();
                if (butacasIds.Any(id => !idsSala.Contains(id)))
                    throw new DatosLlegaronErradosException("Alguna butaca no pertenece a la sala de la función.");

                // Chequeo rápido para dar el mensaje bien; la garantía real es el índice único.
                HashSet<int> ocupadas = await _funcionRepo.ObtenerButacasOcupadas(funcion.Id);
                if (butacasIds.Any(ocupadas.Contains))
                    throw new ButacaOcupadaException();

                venta.FuncionId = funcion.Id;
                foreach (int butacaId in butacasIds)
                {
                    venta.Entradas.Add(new EntradaVenta
                    {
                        ButacaId = butacaId,
                        FuncionId = funcion.Id,
                        Precio = funcion.PrecioEntrada,
                        Activa = true
                    });
                }
                total += funcion.PrecioEntrada * butacasIds.Count;
            }

            if (haySnacks)
            {
                var (detalles, totalSnacks, consumo) = await PrepararSnacks(items);
                foreach (DetalleVentaSnack d in detalles) venta.Snacks.Add(d);
                total += totalSnacks;

                await _inventario.Descontar(consumo);
                venta.ConsumosStock = consumo
                    .Select(c => new ConsumoStockVenta { SnackId = c.Key, Cantidad = c.Value }).ToList();
            }

            venta.Total = total;
            return venta;
        }
        // Cambia el estado de forma atómica y, solo si lo logró, libera butacas y devuelve stock.
        // Devuelve false si otro proceso ya había cambiado la venta. Llamar dentro de una transacción.

        private async Task<bool> CerrarVenta(int ventaId, EstadoVenta desde, EstadoVenta hasta,
                EstadoPago pagosDesde, EstadoPago pagosHasta, bool soloSiVencida = false)
        {
            if (!await _repo.TryCambiarEstado(ventaId, desde, hasta, soloSiVencida))
                return false;

            await _repo.ActualizarEstadoPagos(ventaId, pagosDesde, pagosHasta);
            await _repo.DesactivarEntradas(ventaId);
            await _inventario.Devolver(await _repo.ObtenerConsumoStock(ventaId)); // exactamente lo que se descontó
            return true;
        }
        internal async Task<bool> VencerReserva(int ventaId)
        {
            return await _uow.Ejecutar(async () =>
            {
                bool ok = await CerrarVenta(ventaId, EstadoVenta.Pendiente, EstadoVenta.Cancelada,
                    EstadoPago.Pendiente, EstadoPago.Rechazado, soloSiVencida: true);
                if (ok) await _uow.Auditar(null, "ReservaVencida", nameof(Venta), ventaId);
                return ok;
            });
        }

        internal async Task<ResultadoPagoAplicado> AplicarResultadoPago(
                int ventaId, int pagoId, ResultadoPasarelaPago resultado, int usuarioId)
        {
            return await _uow.Ejecutar(async () =>
            {
                if (resultado.Estado == EstadoPago.Aprobado)
                {
                    if (await _repo.TryCambiarEstado(ventaId, EstadoVenta.Pendiente, EstadoVenta.Confirmada))
                    {
                        await _repo.ResolverPago(pagoId, EstadoPago.Aprobado, resultado.ReferenciaPago);
                        await _uow.Auditar(usuarioId, "PagoAprobado", nameof(Venta), ventaId, resultado.Mensaje);
                        return ResultadoPagoAplicado.Confirmada;
                    }

                    // La venta venció o se canceló mientras la pasarela respondía: se devuelve el dinero.
                    await _repo.ResolverPago(pagoId, EstadoPago.Reembolsado, resultado.ReferenciaPago);
                    await _uow.Auditar(usuarioId, "PagoReembolsadoPorVencimiento", nameof(Venta), ventaId);
                    return ResultadoPagoAplicado.VencidaReembolsada;
                }

                await _repo.ResolverPago(pagoId, EstadoPago.Rechazado, resultado.ReferenciaPago);
                await CerrarVenta(ventaId, EstadoVenta.Pendiente, EstadoVenta.Cancelada,
                    EstadoPago.Pendiente, EstadoPago.Rechazado);
                await _uow.Auditar(usuarioId, "PagoRechazado", nameof(Venta), ventaId, resultado.Mensaje);
                return ResultadoPagoAplicado.Rechazada;
            });
        }

        private static VentaResponse MapearADto(Venta v) => new(
                v.Id, v.Fecha, v.FechaExpiracion, v.Total, v.ClienteId, v.Cliente?.Nombre,
                v.UsuarioId, v.Usuario?.Username, v.Estado,
                v.Funcion is null ? null : new VentaFuncionResponse(
                    v.Funcion.Id, v.Funcion.PeliculaId, v.Funcion.Pelicula.Titulo, v.Funcion.Sala.Numero, v.Funcion.FechaHora),
                v.Entradas.OrderBy(e => e.Butaca.Fila).ThenBy(e => e.Butaca.Numero)
                    .Select(e => new VentaEntradaResponse(e.ButacaId, e.Butaca.Fila, e.Butaca.Numero, e.Precio)).ToList(),
                v.Snacks.Select(s => new VentaSnackResponse(
                    s.SnackId, s.ComboId, s.Snack?.Nombre ?? s.Combo!.Nombre,
                    s.Cantidad, s.PrecioUnitario, s.PrecioUnitario * s.Cantidad)).ToList());
        private static VentaComprobanteResponse MapearAComprobante(Venta v) => new()
        {
            Id = v.Id,
            Fecha = v.Fecha,
            Estado = v.Estado.ToString(),
            ClienteNombre = v.Cliente?.Nombre,
            Total = v.Total,

            FuncionDescripcion = v.Funcion is null
        ? null
        : $"{v.Funcion.Pelicula.Titulo} - " +
          $"Sala {v.Funcion.Sala.Numero} - " +
          $"{v.Funcion.FechaHora:dd/MM/yyyy HH:mm}",

            Butacas = v.Entradas
        .Where(e => e.Activa)
        .OrderBy(e => e.Butaca.Fila)
        .ThenBy(e => e.Butaca.Numero)
        .Select(e => new ButacaComprobanteResponse
        {
            Identificacion = $"{e.Butaca.Fila}{e.Butaca.Numero}",
            Precio = e.Precio
        })
        .ToList(),

            Snacks = v.Snacks
        .Select(s => new SnackComprobanteResponse
        {
            ProductoNombre = s.Snack?.Nombre ?? s.Combo!.Nombre,
            Cantidad = s.Cantidad,
            PrecioUnitario = s.PrecioUnitario
        })
        .ToList()
        };
        public async Task Anular(int id, int usuarioId)
        {
            _ = await _repo.ObtenerPorId(id)
                ?? throw new RecursoNoExisteException("No existe ninguna venta con ese id.");

            bool anulada = await _uow.Ejecutar(async () =>
            {
                bool ok = await CerrarVenta(id, EstadoVenta.Confirmada, EstadoVenta.Anulada,
                    EstadoPago.Aprobado, EstadoPago.Reembolsado);
                if (ok) await _uow.Auditar(usuarioId, "VentaAnulada", nameof(Venta), id);
                return ok;
            });

            if (!anulada)
                throw new DatosLlegaronErradosException("Solo se pueden anular ventas confirmadas.");
        }
        public async Task CancelarMiCompra(int id, int usuarioId)
        {
            Cliente? cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId);
            Venta? venta = await _repo.ObtenerPorId(id);
            if (venta is null || cliente is null || venta.ClienteId != cliente.Id)
                throw new RecursoNoExisteException("No existe ninguna compra con ese id.");

            bool cancelada = await _uow.Ejecutar(async () =>
            {
                bool ok = await CerrarVenta(id, EstadoVenta.Pendiente, EstadoVenta.Cancelada,
                    EstadoPago.Pendiente, EstadoPago.Rechazado);
                if (ok) await _uow.Auditar(usuarioId, "CompraCancelada", nameof(Venta), id);
                return ok;
            });

            if (!cancelada)
                throw new DatosLlegaronErradosException("La compra ya no está pendiente.");
        }

        public async Task LiberarVencidas()
        {
            foreach (int id in await _repo.ObtenerIdsVencidas(DateTime.UtcNow))
                await VencerReserva(id);
        }

        // Este, Sol, lo agregué para el tema del ticket del lado de la app.
        public async Task<ComprobanteVentaResponse> ObtenerParaTicket(int id, int usuarioId)
        {
            Cliente? cliente = await _clienteRepo.ObtenerPorUsuarioId(usuarioId);
            Venta? venta = await _repo.ObtenerParaTicket(id);

            if (venta is null || cliente is null || venta.ClienteId != cliente.Id)
            {
                throw new RecursoNoExisteException("No existe ninguna compra con ese id.");
            }

            var pagos = venta.Pagos
                .OrderBy(p => p.Fecha)
                .ThenBy(p => p.Id)
                .Select(p => new PagoComprobanteResponse(
                    p.Monto,
                    p.Metodo,
                    p.ReferenciaPago,
                    p.Estado
                ))
                .ToList();

            return new ComprobanteVentaResponse
            {
                Venta = MapearAComprobante(venta),
                Pagos = pagos
            };
        }

        public async Task<ComprobanteVentaResponse> ObtenerParaTicketAdmin(int id)
        {
            Venta? venta = await _repo.ObtenerParaTicket(id);

            if (venta is null)
            {
                throw new RecursoNoExisteException("No existe ninguna venta con ese id.");
            }

            var pagos = venta.Pagos
                .OrderBy(p => p.Fecha)
                .ThenBy(p => p.Id)
                .Select(p => new PagoComprobanteResponse(
                    p.Monto,
                    p.Metodo,
                    p.ReferenciaPago,
                    p.Estado
                ))
                .ToList();

            return new ComprobanteVentaResponse
            {
                Venta = MapearAComprobante(venta),
                Pagos = pagos
            };
        }

    }
}
