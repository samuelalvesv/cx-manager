namespace CxManager.UnitTests;

public sealed class NpsFormulaTests
{
    [Fact]
    public void NpsFormulas_ShouldMatchConferenceTableFromBusinessRulesSpecification()
    {
        // Dados oficiais de conferência (01-regras-de-negocio.md)
        const int totalNpsResponses = 978;
        const int promoters = 477;
        const int neutrals = 256;
        const int detractors = 245;

        // Cálculo com precisão total (porcentagens cruas)
        double rawPromotersPct = (double)promoters / totalNpsResponses * 100.0;
        double rawNeutralsPct = (double)neutrals / totalNpsResponses * 100.0;
        double rawDetractorsPct = (double)detractors / totalNpsResponses * 100.0;

        // Arredondamento para representação: 1 casa decimal
        double formattedPromotersPct = Math.Round(value: rawPromotersPct, digits: 1, mode: MidpointRounding.AwayFromZero);
        double formattedNeutralsPct = Math.Round(value: rawNeutralsPct, digits: 1, mode: MidpointRounding.AwayFromZero);
        double formattedDetractorsPct = Math.Round(value: rawDetractorsPct, digits: 1, mode: MidpointRounding.AwayFromZero);

        Assert.Equal(expected: 48.8, actual: formattedPromotersPct);
        Assert.Equal(expected: 26.2, actual: formattedNeutralsPct);
        Assert.Equal(expected: 25.1, actual: formattedDetractorsPct);

        // NPS Score: diferença das porcentagens cruas (não arredondadas) convertida para inteiro
        double rawDifference = rawPromotersPct - rawDetractorsPct;
        int npsScore = (int)Math.Round(value: rawDifference, mode: MidpointRounding.AwayFromZero);

        Assert.Equal(expected: 24, actual: npsScore);
    }
}
