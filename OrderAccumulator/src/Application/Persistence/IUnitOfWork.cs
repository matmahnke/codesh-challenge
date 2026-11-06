namespace Application.Persistence
{
    public interface IUnitOfWork
    {
        Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
    }
}