using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Surveys;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using SurveyResponse = CxManager.Contract.Responses.Surveys.SurveyResponse;
using ValidationException = CxManager.Domain.Exceptions.ValidationException;

namespace CxManager.Application.UseCases.Surveys.Commands.UpdateSurvey;

public sealed class UpdateSurveyUseCase(
    ISurveyRepository surveyRepository,
    ICompanyContext companyContext,
    IValidator<UpdateSurveyRequest> validator) : IUpdateSurveyUseCase
{
    public async Task<SurveyResponse> ExecuteAsync(
        long id,
        UpdateSurveyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await validator.ValidateAsync(
            instance: request,
            cancellation: cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(message: validationResult.Errors[0].ErrorMessage);

        long companyId = companyContext.CompanyId;

        Survey? survey = await surveyRepository.GetByIdAsync(
            id: id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (survey is null || survey.IsDeleted)
            throw new EntityNotFoundException(message: "Pesquisa não encontrada.");

        survey.Update(name: request.Name);

        await surveyRepository.UpdateAsync(
            survey: survey,
            cancellationToken: cancellationToken);

        return SurveyMapper.ToResponse(survey: survey);
    }
}
