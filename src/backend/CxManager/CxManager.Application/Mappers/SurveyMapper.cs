using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using SurveyResponseContract = CxManager.Contract.Responses.Surveys.SurveyResponse;

namespace CxManager.Application.Mappers;

public static class SurveyMapper
{
    public static SurveyResponseContract ToResponse(Survey survey, int responsesCount = 0) =>
        new()
        {
            Id = survey.Id,
            Name = survey.Name,
            Type = MapSurveyTypeToString(type: survey.Type),
            CreatedAt = survey.CreatedAt,
            ResponsesCount = responsesCount
        };

    public static string MapSurveyTypeToString(SurveyType type) =>
        type switch
        {
            SurveyType.Nps => "NPS",
            SurveyType.Csat => "CSAT",
            _ => type.ToString().ToUpperInvariant()
        };
}
