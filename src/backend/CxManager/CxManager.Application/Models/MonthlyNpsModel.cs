namespace CxManager.Application.Models;

public sealed record MonthlyNpsModel
{
    public required string Month { get; init; }
    public required int ResponsesCount { get; init; }
    public required int PromotersCount { get; init; }
    public required int NeutralsCount { get; init; }
    public required int DetractorsCount { get; init; }
    public required int NpsScore { get; init; }
}
