using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Feedbacks.Commands.CreateFeedback;

public sealed class CreateFeedbackUseCase(
    IFeedbackRepository feedbackRepository,
    ISurveyRepository surveyRepository,
    IContactRepository contactRepository,
    IFeedbackQueryService feedbackQueryService,
    IAnalyticsCache analyticsCache,
    ICompanyContext companyContext) : ICreateFeedbackUseCase
{
    public async Task<FeedbackResponse> ExecuteAsync(CreateFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Survey? survey = await surveyRepository.GetByIdAsync(
            id: request.SurveyId,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (survey is null)
            throw new EntityNotFoundException(message: "Pesquisa não encontrada para a empresa ativa.");

        Contact? contact = await contactRepository.GetByIdAsync(
            id: request.ContactId,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (contact is null)
            throw new EntityNotFoundException(message: "Contato não encontrado para a empresa ativa.");

        Feedback.ValidateScoreForSurveyType(score: request.Score, type: survey.Type);

        var feedback = Feedback.Create(
            surveyId: request.SurveyId,
            contactId: request.ContactId,
            score: request.Score,
            channel: request.Channel,
            comment: request.Comment);

        long id = await feedbackRepository.CreateAsync(
            feedback: feedback,
            cancellationToken: cancellationToken);

        analyticsCache.InvalidateCompany(companyId: companyId);

        FeedbackReadModel? detail = await feedbackQueryService.GetDetailByIdAsync(
            id: id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        return FeedbackMapper.ToResponse(model: detail!);
    }
}
