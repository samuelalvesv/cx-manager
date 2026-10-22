using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using CxManager.Domain.Exceptions;

namespace CxManager.UnitTests;

public sealed class FeedbackDomainTests
{
    [Theory]
    [InlineData("whatsapp")]
    [InlineData("email")]
    [InlineData("link")]
    [InlineData("WHATSAPP")]
    [InlineData("Email")]
    public void Create_WithValidParameters_ShouldCreateFeedback(string channel)
    {
        var feedback = Feedback.Create(
            surveyId: 1,
            contactId: 2,
            score: 10,
            channel: channel,
            comment: "Ótimo atendimento!");

        Assert.Equal(expected: 0, actual: feedback.Id);
        Assert.Equal(expected: 1, actual: feedback.SurveyId);
        Assert.Equal(expected: 2, actual: feedback.ContactId);
        Assert.Equal(expected: 10, actual: feedback.Score);
        Assert.Equal(expected: channel.ToLowerInvariant(), actual: feedback.Channel);
        Assert.Equal(expected: "Ótimo atendimento!", actual: feedback.Comment);
        Assert.Null(@object: feedback.DeletedAt);
        Assert.False(condition: feedback.IsDeleted);
        Assert.True(condition: feedback.RespondedAt <= DateTimeOffset.UtcNow);
    }

    [Theory]
    [InlineData("sms")]
    [InlineData("telegram")]
    [InlineData("telefone")]
    [InlineData("invalido")]
    public void Create_WithUnsupportedChannel_ShouldThrowValidationException(string unsupportedChannel)
    {
        Assert.Throws<ValidationException>(testCode: () => Feedback.Create(
            surveyId: 1,
            contactId: 2,
            score: 10,
            channel: unsupportedChannel));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidSurveyId_ShouldThrowValidationException(long invalidSurveyId)
    {
        Assert.Throws<ValidationException>(testCode: () => Feedback.Create(
            surveyId: invalidSurveyId,
            contactId: 2,
            score: 10,
            channel: "whatsapp"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithInvalidContactId_ShouldThrowValidationException(long invalidContactId)
    {
        Assert.Throws<ValidationException>(testCode: () => Feedback.Create(
            surveyId: 1,
            contactId: invalidContactId,
            score: 10,
            channel: "whatsapp"));
    }

    [Fact]
    public void Create_WithCommentExceedingMaximumLength_ShouldThrowValidationException()
    {
        string longComment = new('A', count: 1001);

        Assert.Throws<ValidationException>(testCode: () => Feedback.Create(
            surveyId: 1,
            contactId: 2,
            score: 10,
            channel: "whatsapp",
            comment: longComment));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public void ValidateScoreForSurveyType_Nps_ValidScores_ShouldNotThrow(int validScore)
        => Feedback.ValidateScoreForSurveyType(score: validScore, type: SurveyType.Nps);

    [Theory]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(100)]
    public void ValidateScoreForSurveyType_Nps_InvalidScores_ShouldThrowValidationException(int invalidScore)
    {
        ValidationException ex = Assert.Throws<ValidationException>(testCode: () =>
            Feedback.ValidateScoreForSurveyType(score: invalidScore, type: SurveyType.Nps));

        Assert.Equal(expected: "A nota para pesquisas NPS deve estar entre 0 e 10.", actual: ex.Message);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public void ValidateScoreForSurveyType_Csat_ValidScores_ShouldNotThrow(int validScore)
        => Feedback.ValidateScoreForSurveyType(score: validScore, type: SurveyType.Csat);

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    [InlineData(-1)]
    public void ValidateScoreForSurveyType_Csat_InvalidScores_ShouldThrowValidationException(int invalidScore)
    {
        ValidationException ex = Assert.Throws<ValidationException>(testCode: ()
            => Feedback.ValidateScoreForSurveyType(score: invalidScore, type: SurveyType.Csat));

        Assert.Equal(expected: "A nota para pesquisas CSAT deve estar entre 1 e 5.", actual: ex.Message);
    }

    [Fact]
    public void Update_WithValidScoreAndComment_ShouldUpdateProperties()
    {
        var feedback = Feedback.Create(
            surveyId: 1,
            contactId: 2,
            score: 5,
            channel: "whatsapp",
            comment: "Comentário inicial");

        feedback.Update(score: 9, comment: "Comentário atualizado", surveyType: SurveyType.Nps);

        Assert.Equal(expected: 9, actual: feedback.Score);
        Assert.Equal(expected: "Comentário atualizado", actual: feedback.Comment);
    }

    [Fact]
    public void Update_WithInvalidScoreForSurveyType_ShouldThrowValidationException()
    {
        var feedback = Feedback.Create(
            surveyId: 1,
            contactId: 2,
            score: 4,
            channel: "email");

        Assert.Throws<ValidationException>(testCode: () =>
            feedback.Update(score: 6, comment: "Nota CSAT inválida", surveyType: SurveyType.Csat));
    }

    [Fact]
    public void SoftDelete_ShouldSetDeletedAtAndIsDeleted()
    {
        var feedback = Feedback.Create(
            surveyId: 1,
            contactId: 2,
            score: 8,
            channel: "link");

        feedback.SoftDelete();

        Assert.NotNull(@object: feedback.DeletedAt);
        Assert.True(condition: feedback.IsDeleted);
    }

    [Fact]
    public void Reconstitute_ShouldPreserveProperties()
    {
        DateTimeOffset respondedAt = DateTimeOffset.UtcNow.AddDays(days: -10);
        DateTimeOffset? deletedAt = DateTimeOffset.UtcNow.AddDays(days: -1);

        var feedback = Feedback.Reconstitute(
            id: 99,
            surveyId: 1,
            contactId: 2,
            score: 7,
            comment: "Reconstituído",
            channel: "whatsapp",
            respondedAt: respondedAt,
            deletedAt: deletedAt);

        Assert.Equal(expected: 99, actual: feedback.Id);
        Assert.Equal(expected: 1, actual: feedback.SurveyId);
        Assert.Equal(expected: 2, actual: feedback.ContactId);
        Assert.Equal(expected: 7, actual: feedback.Score);
        Assert.Equal(expected: "Reconstituído", actual: feedback.Comment);
        Assert.Equal(expected: "whatsapp", actual: feedback.Channel);
        Assert.Equal(expected: respondedAt, actual: feedback.RespondedAt);
        Assert.Equal(expected: deletedAt, actual: feedback.DeletedAt);
        Assert.True(condition: feedback.IsDeleted);
    }
}
