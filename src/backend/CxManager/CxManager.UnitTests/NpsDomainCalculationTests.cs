using CxManager.Domain.Analytics;

namespace CxManager.UnitTests;

public sealed class NpsDomainCalculationTests
{
    [Fact]
    public void CalculateScore_WithSeedDataCounts_ShouldReturnExactExpectedScore()
    {
        // 477 promotores, 245 detratores em 978 respostas NPS
        int score = NpsCalculator.CalculateScore(
            promotersCount: 477,
            detractorsCount: 245,
            totalNpsResponses: 978);

        Assert.Equal(expected: 24, actual: score);
    }

    [Fact]
    public void CalculateScore_WithZeroResponses_ShouldReturnZero()
    {
        int score = NpsCalculator.CalculateScore(
            promotersCount: 0,
            detractorsCount: 0,
            totalNpsResponses: 0);

        Assert.Equal(expected: 0, actual: score);
    }

    [Fact]
    public void CalculateScore_ShouldUseAwayFromZeroRounding()
    {
        // promoters: 500/1000 = 50%, detractors: 265/1000 = 26.5% -> diff = 23.5 -> 24
        int scoreWith500 = NpsCalculator.CalculateScore(
            promotersCount: 500,
            detractorsCount: 265,
            totalNpsResponses: 1000);
        Assert.Equal(expected: 24, actual: scoreWith500);

        // Diferença crua negativa = 26.5% - 50.0% = -23.5% -> deve arredondar para -24
        int scoreNegative = NpsCalculator.CalculateScore(
            promotersCount: 265,
            detractorsCount: 500,
            totalNpsResponses: 1000);
        Assert.Equal(expected: -24, actual: scoreNegative);
    }

    [Fact]
    public void CalculatePercent_WithSeedDataCounts_ShouldReturnFormattedToOneDecimal()
    {
        double promotersPercent = NpsCalculator.CalculatePercent(count: 477, totalNpsResponses: 978);
        double neutralsPercent = NpsCalculator.CalculatePercent(count: 256, totalNpsResponses: 978);
        double detractorsPercent = NpsCalculator.CalculatePercent(count: 245, totalNpsResponses: 978);

        Assert.Equal(expected: 48.8, actual: promotersPercent);
        Assert.Equal(expected: 26.2, actual: neutralsPercent);
        Assert.Equal(expected: 25.1, actual: detractorsPercent);
    }

    [Fact]
    public void CalculatePercent_WithZeroTotal_ShouldReturnZero()
    {
        double percent = NpsCalculator.CalculatePercent(count: 0, totalNpsResponses: 0);
        Assert.Equal(expected: 0.0, actual: percent);
    }

    [Fact]
    public void RoundCsatAverage_WithNull_ShouldReturnNull()
    {
        double? csatAvg = NpsCalculator.RoundCsatAverage(rawAverage: null);
        Assert.Null(@object: csatAvg);
    }

    [Fact]
    public void RoundCsatAverage_WithValues_ShouldRoundToTwoDecimalsAwayFromZero()
    {
        // 3.914 -> 3.91
        double? csatAvg1 = NpsCalculator.RoundCsatAverage(rawAverage: 3.914);
        Assert.Equal(expected: 3.91, actual: csatAvg1);

        // 3.915 -> 3.92 (AwayFromZero)
        double? csatAvg2 = NpsCalculator.RoundCsatAverage(rawAverage: 3.915);
        Assert.Equal(expected: 3.92, actual: csatAvg2);
    }
}
