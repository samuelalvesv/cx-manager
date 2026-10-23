namespace CxManager.Contract.Responses.Analytics;

public sealed record MonthlyNpsResponse
{
    public required string Month { get; init; }
    public required int Responses { get; init; }
    public required int NpsScore { get; init; }
}
