using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Application.Models;
using CxManager.Contract.Requests.Feedbacks;
using CxManager.Contract.Responses.Feedbacks;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Feedbacks.Commands.UpdateFeedback;

public sealed class UpdateFeedbackUseCase(
    IFeedbackRepository feedbackRepository,
    ISurveyRepository surveyRepository,
    IFeedbackQueryService feedbackQueryService,
    IAnalyticsCache analyticsCache,
    ICompanyContext companyContext) : IUpdateFeedbackUseCase
{
    public async Task<FeedbackResponse> ExecuteAsync(UpdateFeedbackRequest request, CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        Feedback? feedback = await feedbackRepository.GetByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (feedback is null)
            throw new EntityNotFoundException(message: "Resposta não encontrada para a empresa ativa.");

        Survey? survey = await surveyRepository.GetByIdAsync(
            id: feedback.SurveyId,
            companyId: companyId,
            cancellationToken: cancellationToken);

        if (survey is null)
            throw new EntityNotFoundException(message: "Pesquisa vinculada à resposta não encontrada.");

        feedback.Update(score: request.Score, comment: request.Comment, surveyType: survey.Type);

        await feedbackRepository.UpdateAsync(
            feedback: feedback,
            cancellationToken: cancellationToken);

        analyticsCache.InvalidateCompany(companyId: companyId);

        FeedbackReadModel? detail = await feedbackQueryService.GetDetailByIdAsync(
            id: request.Id,
            companyId: companyId,
            cancellationToken: cancellationToken);

        return FeedbackMapper.ToResponse(model: detail!);
    }
}
