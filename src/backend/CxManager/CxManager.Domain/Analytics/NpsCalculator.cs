namespace CxManager.Domain.Analytics;

public static class NpsCalculator
{
    public static int CalculateScore(int promotersCount, int detractorsCount, int totalNpsResponses)
    {
        if (totalNpsResponses <= 0)
            return 0;

        double rawPromotersPct = (double)promotersCount / totalNpsResponses * 100.0;
        double rawDetractorsPct = (double)detractorsCount / totalNpsResponses * 100.0;
        double rawDiff = rawPromotersPct - rawDetractorsPct;

        return (int)Math.Round(value: rawDiff, digits: 0, mode: MidpointRounding.AwayFromZero);
    }

    public static double CalculatePercent(int count, int totalNpsResponses)
    {
        if (totalNpsResponses <= 0)
            return 0.0;

        double rawPct = (double)count / totalNpsResponses * 100.0;
        return Math.Round(value: rawPct, digits: 1, mode: MidpointRounding.AwayFromZero);
    }

    public static double? CalculateCsatAverage(double rawSum, int csatResponsesCount)
    {
        if (csatResponsesCount <= 0)
            return null;

        double rawAvg = rawSum / csatResponsesCount;
        return Math.Round(value: rawAvg, digits: 2, mode: MidpointRounding.AwayFromZero);
    }
}
