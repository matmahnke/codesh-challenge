namespace Application.Persistence
{
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken ct = default);
    }
}