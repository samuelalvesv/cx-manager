using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Domain.Entities;
using CxManager.Domain.Interfaces;
using SurveyResponse = CxManager.Contract.Responses.Surveys.SurveyResponse;

namespace CxManager.Application.UseCases.Surveys.Queries;

public sealed class GetSurveysUseCase(
    ISurveyRepository surveyRepository,
    ICompanyContext companyContext) : IGetSurveysUseCase
{
    public async Task<IReadOnlyList<SurveyResponse>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        IReadOnlyList<Survey> surveys = await surveyRepository.GetAllActiveByCompanyAsync(
            companyId: companyId,
            cancellationToken: cancellationToken);

        return SurveyMapper.ToResponseList(surveys: surveys);
    }
}
