using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using Riok.Mapperly.Abstractions;
using SurveyResponseContract = CxManager.Contract.Responses.Surveys.SurveyResponse;

namespace CxManager.Application.Mappers;

[Mapper]
public static partial class SurveyMapper
{
    [MapperIgnoreSource(nameof(Survey.CompanyId))]
    [MapperIgnoreSource(nameof(Survey.DeletedAt))]
    [MapperIgnoreSource(nameof(Survey.IsDeleted))]
    [MapProperty(nameof(Survey.Type), nameof(SurveyResponseContract.Type), Use = nameof(MapSurveyTypeToString))]
    public static partial SurveyResponseContract ToResponse(Survey survey);

    public static partial IReadOnlyList<SurveyResponseContract> ToResponseList(IReadOnlyList<Survey> surveys);

    public static string MapSurveyTypeToString(SurveyType type) =>
        type switch
        {
            SurveyType.Nps => "NPS",
            SurveyType.Csat => "CSAT",
            _ => type.ToString().ToUpperInvariant()
        };
}
