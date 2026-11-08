using Cinemax.Backend.Data;
using Cinemax.Backend.Excepciones;
using Cinemax.Backend.Models;
using Cinemax.Backend.Models.ModeloAuxiliar;
using Cinemax.Backend.Models.ModeloAuxiliar.Query.Venta;
using Cinemax.Backend.Repositories;
using Cinemax.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Cinemax.Backend.Implementacion
{
    public class VentaRepositoryPsqlEF : IVentaRepository
    {
    private readonly DataContext _dataContext;
    public VentaRepositoryPsqlEF(DataContext dataContext)
    {
        _dataContext = dataContext;

    }
    public async Task<PaginadoResponse<Venta>> ObtenerTodas(VentaQueryParametros parametros)
    {
        IQueryable<Venta> query = _dataContext.Ventas
            .Include(v => v.Cliente)
            .AsNoTracking();

        if (parametros.ClienteId.HasValue)
            query = query.Where(v => v.ClienteId == parametros.ClienteId.Value);

        if (parametros.Estado.HasValue)
            query = query.Where(v => v.Estado == parametros.Estado.Value);

        if (!string.IsNullOrWhiteSpace(parametros.Buscar))
        {
            string buscar = parametros.Buscar.ToLower();
            query = query.Where(v => v.Cliente != null && v.Cliente.Nombre.ToLower().Contains(buscar));
        }

        bool descendente = parametros.Direccion?.Equals("desc", StringComparison.OrdinalIgnoreCase) != false;
        query = parametros.OrdenarPor?.ToLower() switch
        {
            "total" => descendente ? query.OrderByDescending(v => v.Total).ThenByDescending(v => v.Id) : query.OrderBy(v => v.Total).ThenBy(v => v.Id),
            "fecha" => descendente ? query.OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Id) : query.OrderBy(v => v.Fecha).ThenBy(v => v.Id),
            _ => descendente ? query.OrderByDescending(v => v.Fecha).ThenByDescending(v => v.Id) : query.OrderBy(v => v.Fecha).ThenBy(v => v.Id)
        };

        int totalRegistros = await query.CountAsync();
        List<Venta> ventas = await query
            .Skip((parametros.NumeroPagina - 1) * parametros.TamanoPagina)
            .Take(parametros.TamanoPagina)
            .ToListAsync();

        return new PaginadoResponse<Venta>(ventas, parametros.NumeroPagina, parametros.TamanoPagina, totalRegistros);
    }
        public async Task<Venta?> ObtenerPorId(int id)
        {
            // AsNoTracking: siempre trae el estado actual de la base, no una copia vieja del contexto.
            return await _dataContext.Ventas
                .AsNoTracking()
                .Include(v => v.Cliente)
                .Include(v => v.Usuario)
                .Include(v => v.Funcion).ThenInclude(f => f!.Pelicula)
                .Include(v => v.Funcion).ThenInclude(f => f!.Sala)
                .Include(v => v.Entradas).ThenInclude(e => e.Butaca)
                .Include(v => v.Snacks).ThenInclude(s => s.Snack)
                .Include(v => v.Snacks).ThenInclude(s => s.Combo)
                .Include(v => v.Pagos)
                .AsSplitQuery()
                .FirstOrDefaultAsync(v => v.Id == id);
        }
        public Task<Venta?> ObtenerParaTicket(int id) => ObtenerPorId(id);
        public async Task Crear(Venta venta)
        {
            _dataContext.Ventas.Add(venta); // entradas, snacks y consumos viajan en el grafo
            try { await _dataContext.SaveChangesAsync(); }
            catch (DbUpdateException e) when (EsViolacionUnica(e, "UX_EntradaVenta_Funcion_Butaca_Activa"))
            {
                throw new ButacaOcupadaException(); // otra persona se llevó la butaca entre el chequeo y el insert
            }
        }
        public async Task ActualizarEstadoPagos(int ventaId, EstadoPago desde, EstadoPago hasta)
        {
            await _dataContext.Pagos
                .Where(p => p.VentaId == ventaId && p.Estado == desde)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.Estado, hasta));
        }
        public async Task<bool> TienePendiente(int clienteId, DateTime ahora)
        {
            return await _dataContext.Ventas.AnyAsync(v =>
                v.ClienteId == clienteId && v.Estado == EstadoVenta.Pendiente && v.FechaExpiracion > ahora);
        }
        public async Task ResolverPago(int pagoId, EstadoPago estado, string? referencia)
        {
            await _dataContext.Pagos
                .Where(p => p.Id == pagoId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(p => p.Estado, estado)
                    .SetProperty(p => p.ReferenciaPago, referencia)
                    .SetProperty(p => p.Fecha, DateTime.UtcNow));
        }
        public async Task<int> CrearPagoPendiente(int ventaId, MetodoPago metodo, decimal monto)
        {
            Pago pago = new() { VentaId = ventaId, Metodo = metodo, Monto = monto, Estado = EstadoPago.Pendiente, Fecha = DateTime.UtcNow };
            _dataContext.Pagos.Add(pago);
            try { await _dataContext.SaveChangesAsync(); }
            catch (DbUpdateException e) when (EsViolacionUnica(e, "UX_Pago_Venta_Pendiente"))
            {
                throw new RecursoExistenteException("Ya hay un pago en proceso para esta compra.");
            }
            return pago.Id;
        }
        public async Task<Dictionary<int, int>> ObtenerConsumoStock(int ventaId)
        {
            return await _dataContext.ConsumosStockVenta
                .Where(c => c.VentaId == ventaId)
                .ToDictionaryAsync(c => c.SnackId, c => c.Cantidad);
        }
        public async Task<List<int>> ObtenerIdsVencidas(DateTime ahora)
        {
            return await _dataContext.Ventas
                .Where(v => v.Estado == EstadoVenta.Pendiente && v.FechaExpiracion != null && v.FechaExpiracion <= ahora)
                .Select(v => v.Id)
                .ToListAsync();
        }
        public async Task DesactivarEntradas(int ventaId)
        {
            await _dataContext.EntradasVenta
                .Where(e => e.VentaId == ventaId && e.Activa)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.Activa, false));
        }
        public async Task<bool> TryCambiarEstado(int ventaId, EstadoVenta desde, EstadoVenta hasta, bool soloSiVencida = false)
        {
            IQueryable<Venta> q = _dataContext.Ventas.Where(v => v.Id == ventaId && v.Estado == desde);
            if (soloSiVencida)
                q = q.Where(v => v.FechaExpiracion != null && v.FechaExpiracion <= DateTime.UtcNow);

            int filas = await q.ExecuteUpdateAsync(s => s
                .SetProperty(v => v.Estado, hasta)
                .SetProperty(v => v.FechaExpiracion, (DateTime?)null));

            return filas == 1; // false = otro proceso ya cambió la venta
        }
        private static bool EsViolacionUnica(DbUpdateException e, string indice) =>
    e.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation } pg
    && pg.ConstraintName == indice;   // using Npgsql;

        public async Task GuardarCambios()
        {
            await _dataContext.SaveChangesAsync();
        }

    }

}
