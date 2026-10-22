using CxManager.Contract.Requests.Feedbacks;
using FluentValidation;

namespace CxManager.Application.Validators.Feedbacks;

public sealed class CreateFeedbackRequestValidator : AbstractValidator<CreateFeedbackRequest>
{
    public CreateFeedbackRequestValidator()
    {
        RuleFor(expression: static x => x.SurveyId)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "O identificador da pesquisa deve ser positivo.");

        RuleFor(expression: static x => x.ContactId)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "O identificador do contato deve ser positivo.");

        RuleFor(expression: static x => x.Score)
            .InclusiveBetween(from: 0, to: 10).WithMessage(errorMessage: "A nota deve estar entre 0 e 10.");

        RuleFor(expression: static x => x.Channel)
            .NotEmpty().WithMessage(errorMessage: "O canal é obrigatório.")
            .Must(predicate: static channel =>
                string.Equals(channel?.Trim(), "whatsapp", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(channel?.Trim(), "email", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(channel?.Trim(), "link", StringComparison.OrdinalIgnoreCase))
            .WithMessage(errorMessage: "O canal deve ser 'whatsapp', 'email' ou 'link'.");

        RuleFor(expression: static x => x.Comment)
            .MaximumLength(maximumLength: 1000).WithMessage(errorMessage: "O comentário não pode exceder 1000 caracteres.");
    }
}
