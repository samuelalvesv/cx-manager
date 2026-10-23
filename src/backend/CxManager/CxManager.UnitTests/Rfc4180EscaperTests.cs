using CxManager.Domain.Export;

namespace CxManager.UnitTests;

public sealed class Rfc4180EscaperTests
{
    [Fact]
    public void Escape_WhenNullOrEmpty_ShouldReturnEmptyString()
    {
        Assert.Equal(expected: string.Empty, actual: Rfc4180Escaper.Escape(value: null));
        Assert.Equal(expected: string.Empty, actual: Rfc4180Escaper.Escape(value: string.Empty));
    }

    [Fact]
    public void Escape_WhenPlainString_ShouldReturnSameStringWithoutQuotes()
    {
        const string input = "Normal text without specials 123";
        string result = Rfc4180Escaper.Escape(value: input);

        Assert.Equal(expected: input, actual: result);
    }

    [Fact]
    public void Escape_WhenContainsComma_ShouldWrapInQuotes()
    {
        const string input = "Vita Bem-Estar, Unidade Centro";
        string result = Rfc4180Escaper.Escape(value: input);

        Assert.Equal(expected: "\"Vita Bem-Estar, Unidade Centro\"", actual: result);
    }

    [Fact]
    public void Escape_WhenContainsDoubleQuotes_ShouldDoubleQuotesAndWrap()
    {
        const string input = "Comentário com \"aspas\" do aluno";
        string result = Rfc4180Escaper.Escape(value: input);

        Assert.Equal(expected: "\"Comentário com \"\"aspas\"\" do aluno\"", actual: result);
    }

    [Theory]
    [InlineData("Linha 1\nLinha 2")]
    [InlineData("Linha 1\r\nLinha 2")]
    [InlineData("Linha 1\rLinha 2")]
    public void Escape_WhenContainsLineBreaks_ShouldWrapInQuotes(string input)
    {
        string result = Rfc4180Escaper.Escape(value: input);

        Assert.StartsWith(expectedStartString: "\"", actualString: result, comparisonType: StringComparison.Ordinal);
        Assert.EndsWith(expectedEndString: "\"", actualString: result, comparisonType: StringComparison.Ordinal);
        Assert.Contains(expectedSubstring: input, actualString: result, comparisonType: StringComparison.Ordinal);
    }

    [Fact]
    public void Escape_WhenContainsCommasQuotesAndLineBreaks_ShouldDoubleQuotesAndWrap()
    {
        const string input = "Nota 10, atendimento \"excelente\"!\nRecomendo a todos.";
        string result = Rfc4180Escaper.Escape(value: input);

        Assert.Equal(expected: "\"Nota 10, atendimento \"\"excelente\"\"!\nRecomendo a todos.\"", actual: result);
    }
}
