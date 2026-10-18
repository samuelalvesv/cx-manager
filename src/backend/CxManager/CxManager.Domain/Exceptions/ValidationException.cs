namespace CxManager.Domain.Exceptions;

public sealed class ValidationException : DomainException
{
    public ValidationException()
    {
    }

    public ValidationException(string message) : base(message: message)
    {
    }

    public ValidationException(string message, Exception innerException) : base(message: message, innerException: innerException)
    {
    }
}
