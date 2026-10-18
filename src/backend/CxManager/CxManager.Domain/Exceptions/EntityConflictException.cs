namespace CxManager.Domain.Exceptions;

public sealed class EntityConflictException : DomainException
{
    public EntityConflictException()
    {
    }

    public EntityConflictException(string message) : base(message: message)
    {
    }

    public EntityConflictException(string message, Exception innerException) : base(message: message, innerException: innerException)
    {
    }
}
