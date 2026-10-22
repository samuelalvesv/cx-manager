using CxManager.Domain.Enums;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Validation;

namespace CxManager.Domain.Entities;

public sealed class Feedback : Entity
{
    private const int MinChannelLength = 2;
    private const int MaxChannelLength = 30;
    private const int MaxCommentLength = 1000;

    private static readonly HashSet<string> AllowedChannels =
    [
        "whatsapp",
        "email",
        "link"
    ];

    public long SurveyId { get; private set; }
    public long ContactId { get; private set; }
    public int Score { get; private set; }
    public string? Comment { get; private set; }
    public string Channel { get; private set; }

    public DateTimeOffset RespondedAt => CreatedAt;

    private Feedback(
        long id,
        long surveyId,
        long contactId,
        int score,
        string? comment,
        string channel,
        DateTimeOffset respondedAt,
        DateTimeOffset? deletedAt) : base(id: id, createdAt: respondedAt, deletedAt: deletedAt)
    {
        SurveyId = surveyId;
        ContactId = contactId;
        Score = score;
        Comment = comment;
        Channel = channel;
    }

    public static Feedback Create(
        long surveyId,
        long contactId,
        int score,
        string channel,
        string? comment = null,
        DateTimeOffset? respondedAt = null)
    {
        long validatedSurveyId = DomainValidation.ValidatePositiveId(value: surveyId, fieldName: "pesquisa");
        long validatedContactId = DomainValidation.ValidatePositiveId(value: contactId, fieldName: "contato");
        string validatedChannel = ValidateChannel(channel: channel);
        string? validatedComment = ValidateComment(comment: comment);

        return new Feedback(
            id: 0,
            surveyId: validatedSurveyId,
            contactId: validatedContactId,
            score: score,
            comment: validatedComment,
            channel: validatedChannel,
            respondedAt: respondedAt ?? DateTimeOffset.UtcNow,
            deletedAt: null);
    }

    public static Feedback Reconstitute(
        long id,
        long surveyId,
        long contactId,
        int score,
        string? comment,
        string channel,
        DateTimeOffset respondedAt,
        DateTimeOffset? deletedAt)
    {
        return new Feedback(
            id: id,
            surveyId: surveyId,
            contactId: contactId,
            score: score,
            comment: comment,
            channel: channel,
            respondedAt: respondedAt,
            deletedAt: deletedAt);
    }

    public void Update(int score, string? comment, SurveyType surveyType)
    {
        ValidateScoreForSurveyType(score: score, type: surveyType);
        Score = score;
        Comment = ValidateComment(comment: comment);
    }

    public void UpdateComment(string? comment)
        => Comment = ValidateComment(comment: comment);

    public static void ValidateScoreForSurveyType(int score, SurveyType type)
    {
        switch (type)
        {
            case SurveyType.Nps when score is < 0 or > 10:
                throw new ValidationException(message: "A nota para pesquisas NPS deve estar entre 0 e 10.");
            case SurveyType.Csat when score is < 1 or > 5:
                throw new ValidationException(message: "A nota para pesquisas CSAT deve estar entre 1 e 5.");
        }
    }

    private static string ValidateChannel(string channel)
    {
        string validated = DomainValidation.ValidateRequiredString(
            value: channel,
            fieldName: "canal",
            minLength: MinChannelLength,
            maxLength: MaxChannelLength).ToLowerInvariant();

        if (!AllowedChannels.Contains(item: validated))
            throw new ValidationException(message: "O canal deve ser 'whatsapp', 'email' ou 'link'.");

        return validated;
    }

    private static string? ValidateComment(string? comment) =>
        DomainValidation.ValidateOptionalString(
            value: comment,
            fieldName: "comentário",
            minLength: 1,
            maxLength: MaxCommentLength);
}
