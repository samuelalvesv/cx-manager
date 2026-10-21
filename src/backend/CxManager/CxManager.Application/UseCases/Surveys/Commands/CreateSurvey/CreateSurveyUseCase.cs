using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Surveys;
using CxManager.Domain.Entities;
using CxManager.Domain.Enums;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;
using SurveyResponse = CxManager.Contract.Responses.Surveys.SurveyResponse;

namespace CxManager.Application.UseCases.Surveys.Commands.CreateSurvey;

public sealed class CreateSurveyUseCase(
    ISurveyRepository surveyRepository,
    ICompanyRepository companyRepository,
    ICompanyContext companyContext) : ICreateSurveyUseCase
{
    public async Task<SurveyResponse> ExecuteAsync(CreateSurveyRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        bool companyExists = await companyRepository.ExistsActiveAsync(
            id: companyId,
            cancellationToken: cancellationToken);

        if (!companyExists)
            throw new EntityNotFoundException(message: "Empresa não encontrada.");

        SurveyType surveyType = request.Type.Trim().ToUpperInvariant() switch
        {
            "NPS" => SurveyType.Nps,
            "CSAT" => SurveyType.Csat,
            _ => throw new ValidationException(message: "O tipo da pesquisa deve ser 'NPS' ou 'CSAT'.")
        };

        var survey = Survey.Create(
            companyId: companyId,
            name: request.Name,
            type: surveyType);

        long id = await surveyRepository.CreateAsync(
            survey: survey,
            cancellationToken: cancellationToken);

        var createdSurvey = Survey.Reconstitute(
            id: id,
            companyId: survey.CompanyId,
            name: survey.Name,
            type: survey.Type,
            createdAt: survey.CreatedAt,
            deletedAt: null);

        return SurveyMapper.ToResponse(survey: createdSurvey);
    }
}
