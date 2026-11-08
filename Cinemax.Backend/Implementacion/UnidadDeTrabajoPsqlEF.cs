using Cinemax.Backend.Data;
using Cinemax.Backend.Models;
using Cinemax.Backend.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Cinemax.Backend.Implementacion
{
    public class UnidadDeTrabajoPsqlEF : IUnidadDeTrabajo
    {
        private readonly DataContext _db;
        public UnidadDeTrabajoPsqlEF(DataContext db) => _db = db;

        public async Task<T> Ejecutar<T>(Func<Task<T>> operacion)
        {
            // Si ya hay una transacción abierta, se reutiliza.
            if (_db.Database.CurrentTransaction is not null) return await operacion();

            var estrategia = _db.Database.CreateExecutionStrategy();
            return await estrategia.ExecuteAsync(async () =>
            {
                await using var tx = await _db.Database.BeginTransactionAsync();
                T resultado = await operacion();   // si lanza, el dispose revierte todo
                await tx.CommitAsync();
                return resultado;
            });
        }

        public async Task Auditar(int? usuarioId, string accion, string entidad, int entidadId, string? detalle = null)
        {
            _db.Auditorias.Add(new Auditoria
            {
                UsuarioId = usuarioId,
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Detalle = detalle,
                Fecha = DateTime.UtcNow
            });
            await _db.SaveChangesAsync(); // dentro de la transacción de la operación
        }
    }
}
