using Application.Persistence;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.UnitOfWork
{
    public sealed class UnitOfWork(
        OrderAccumulatorDbContext context) : IUnitOfWork
    {
        private IDbContextTransaction? _transaction;

        public async Task BeginTransactionAsync(
            CancellationToken ct = default)
        {
            _transaction = await context.Database
                .BeginTransactionAsync(ct);
        }

        public Task<int> SaveChangesAsync(
            CancellationToken ct = default)
            => context.SaveChangesAsync(ct);

        public async Task CommitAsync(
            CancellationToken ct = default)
        {
            if (_transaction is null)
                throw new InvalidOperationException(
                    "No active transaction.");

            await context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction is not null)
                await _transaction.DisposeAsync();
        }
    }
}
