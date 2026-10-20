using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;
using SurveyResponse = CxManager.Contract.Responses.Surveys.SurveyResponse;

namespace CxManager.Application.UseCases.Surveys.Queries.GetSurveyById;

public sealed class GetSurveyByIdUseCase(
    ISurveyRepository surveyRepository,
    ICompanyContext companyContext) : IGetSurveyByIdUseCase
{
    public async Task<SurveyResponse> ExecuteAsync(long id, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Survey? survey = await surveyRepository.GetByIdAsync(
            id: id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (survey is null || survey.IsDeleted)
            throw new EntityNotFoundException(message: "Pesquisa não encontrada.");

        return SurveyMapper.ToResponse(survey: survey);
    }
}
