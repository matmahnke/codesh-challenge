namespace Application.Persistence
{
    public sealed class ConcurrencyConflictException(Exception inner)
        : Exception("Conflito de concorrência ao processar a ordem. Tente novamente.", inner);
}