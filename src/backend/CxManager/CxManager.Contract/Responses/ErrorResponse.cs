namespace CxManager.Contract.Responses;

public sealed record ErrorResponse
{
    public required string Error { get; init; }
}
