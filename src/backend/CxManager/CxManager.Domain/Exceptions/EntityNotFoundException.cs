namespace CxManager.Domain.Exceptions;

public sealed class EntityNotFoundException : DomainException
{
    public EntityNotFoundException()
    {
    }

    public EntityNotFoundException(string message) : base(message: message)
    {
    }

    public EntityNotFoundException(string message, Exception innerException) : base(message: message, innerException: innerException)
    {
    }
}
