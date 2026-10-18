namespace CxManager.Domain.Exceptions;

public sealed class MissingCompanyContextException : DomainException
{
    private const string DefaultMessage = "O identificador da empresa é obrigatório.";

    public MissingCompanyContextException() : base(message: DefaultMessage)
    {
    }

    public MissingCompanyContextException(string message) : base(message: message)
    {
    }

    public MissingCompanyContextException(string message, Exception innerException) : base(message: message, innerException: innerException)
    {
    }
}
