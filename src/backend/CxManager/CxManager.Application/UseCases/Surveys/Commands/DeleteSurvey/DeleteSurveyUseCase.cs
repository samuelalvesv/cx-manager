using CxManager.Application.Interfaces;
using CxManager.Contract.Requests.Surveys;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Surveys.Commands.DeleteSurvey;

public sealed class DeleteSurveyUseCase(
    ISurveyRepository surveyRepository,
    ICompanyContext companyContext) : IDeleteSurveyUseCase
{
    public async Task ExecuteAsync(DeleteSurveyRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Survey? survey = await surveyRepository.GetByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (survey is null || survey.IsDeleted)
            throw new EntityNotFoundException(message: "Pesquisa não encontrada.");

        survey.SoftDelete();

        await surveyRepository.DeleteAsync(
            id: survey.Id,
            companyId: companyId,
            deletedAt: survey.DeletedAt!.Value,
            cancellationToken: cancellationToken);
    }
}
