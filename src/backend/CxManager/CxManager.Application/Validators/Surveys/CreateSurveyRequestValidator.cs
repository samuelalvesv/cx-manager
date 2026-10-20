using CxManager.Contract.Requests.Surveys;
using FluentValidation;

namespace CxManager.Application.Validators.Surveys;

public sealed class CreateSurveyRequestValidator : AbstractValidator<CreateSurveyRequest>
{
    public CreateSurveyRequestValidator()
    {
        RuleFor(expression: static x => x.Name)
            .NotEmpty().WithMessage(errorMessage: "O nome da pesquisa é obrigatório.")
            .Length(min: 2, max: 100).WithMessage(errorMessage: "O nome da pesquisa deve ter entre 2 e 100 caracteres.");

        RuleFor(expression: static x => x.Type)
            .NotEmpty().WithMessage(errorMessage: "O tipo da pesquisa é obrigatório.")
            .Must(predicate: static type =>
                string.Equals(type?.Trim(), "NPS", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(type?.Trim(), "CSAT", StringComparison.OrdinalIgnoreCase))
            .WithMessage(errorMessage: "O tipo da pesquisa deve ser 'NPS' ou 'CSAT'.");
    }
}
