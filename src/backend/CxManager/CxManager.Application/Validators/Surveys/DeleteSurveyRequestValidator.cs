using CxManager.Contract.Requests.Surveys;
using FluentValidation;

namespace CxManager.Application.Validators.Surveys;

public sealed class DeleteSurveyRequestValidator : AbstractValidator<DeleteSurveyRequest>
{
    public DeleteSurveyRequestValidator()
    {
        RuleFor(expression: static x => x.Id)
            .GreaterThan(valueToCompare: 0)
            .WithMessage(errorMessage: "O identificador da pesquisa deve ser maior que zero.");
    }
}
