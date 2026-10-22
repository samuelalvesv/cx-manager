using CxManager.Contract.Requests.Analytics;
using FluentValidation;

namespace CxManager.Application.Validators.Analytics;

public sealed class GetAnalyticsSummaryRequestValidator : AbstractValidator<GetAnalyticsSummaryRequest>
{
    public GetAnalyticsSummaryRequestValidator()
    {
        RuleFor(expression: static x => x.SurveyId)
            .GreaterThan(valueToCompare: 0)
            .When(predicate: static x => x.SurveyId.HasValue)
            .WithMessage(errorMessage: "O identificador da pesquisa deve ser positivo.");

        RuleFor(expression: static x => x.From)
            .LessThanOrEqualTo(expression: static x => x.To)
            .When(predicate: static x => x.From.HasValue && x.To.HasValue)
            .WithMessage(errorMessage: "A data inicial não pode ser posterior à data final.");
    }
}
