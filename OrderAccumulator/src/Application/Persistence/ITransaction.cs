namespace Application.Persistence
{
    /// <summary>
    /// Transação de banco. Descartar sem chamar <see cref="CommitAsync"/> desfaz tudo (rollback).
    /// </summary>
    public interface ITransaction : IAsyncDisposable
    {
        /// <summary>Persiste as alterações pendentes e confirma a transação.</summary>
        Task CommitAsync(CancellationToken ct = default);
    }
}
