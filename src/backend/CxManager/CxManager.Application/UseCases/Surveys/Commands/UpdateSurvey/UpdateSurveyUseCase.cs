using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Surveys;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;
using SurveyResponse = CxManager.Contract.Responses.Surveys.SurveyResponse;

namespace CxManager.Application.UseCases.Surveys.Commands.UpdateSurvey;

public sealed class UpdateSurveyUseCase(
    ISurveyRepository surveyRepository,
    IAnalyticsCache analyticsCache,
    ICompanyContext companyContext) : IUpdateSurveyUseCase
{
    public async Task<SurveyResponse> ExecuteAsync(
        UpdateSurveyRequest request,
        CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Survey? survey = await surveyRepository.GetByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (survey is null || survey.IsDeleted)
            throw new EntityNotFoundException(message: "Pesquisa não encontrada.");

        survey.Update(name: request.Name);

        await surveyRepository.UpdateAsync(
            survey: survey,
            cancellationToken: cancellationToken);

        analyticsCache.InvalidateCompany(companyId: companyId);

        int responsesCount = await surveyRepository.GetActiveResponsesCountAsync(
            surveyId: survey.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        return SurveyMapper.ToResponse(survey: survey, responsesCount: responsesCount);
    }
}
