namespace Application.Persistence
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken ct = default);

        Task<int> SaveChangesAsync(CancellationToken ct = default);

        Task CommitAsync(CancellationToken ct = default);
    }
}
