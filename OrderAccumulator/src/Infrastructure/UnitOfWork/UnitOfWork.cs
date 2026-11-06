using System.Data;
using Application.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.UnitOfWork
{
    public sealed class UnitOfWork(OrderAccumulatorDbContext context) : IUnitOfWork
    {
        private const int SqliteBusy = 5;
        private const int SqliteLocked = 6;

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default)
        {
            try
            {
                // No SQLite, Serializable emite "BEGIN IMMEDIATE": o lock de escrita é obtido já aqui,
                // serializando o ciclo ler -> validar limite -> gravar entre requisições concorrentes.
                var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
                return new Transaction(context, transaction);
            }
            catch (SqliteException ex) when (IsLockContention(ex))
            {
                throw new ConcurrencyConflictException(ex);
            }
        }

        private static bool IsLockContention(SqliteException ex) =>
            ex.SqliteErrorCode is SqliteBusy or SqliteLocked;

        private sealed class Transaction(DbContext context, IDbContextTransaction transaction) : ITransaction
        {
            public async Task CommitAsync(CancellationToken ct = default)
            {
                try
                {
                    await context.SaveChangesAsync(ct);
                    await transaction.CommitAsync(ct);
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    throw new ConcurrencyConflictException(ex);
                }
                catch (DbUpdateException ex) when (ex.InnerException is SqliteException inner && IsLockContention(inner))
                {
                    throw new ConcurrencyConflictException(ex);
                }
                catch (SqliteException ex) when (IsLockContention(ex))
                {
                    throw new ConcurrencyConflictException(ex);
                }
            }

            public ValueTask DisposeAsync() => transaction.DisposeAsync();
        }
    }
}
