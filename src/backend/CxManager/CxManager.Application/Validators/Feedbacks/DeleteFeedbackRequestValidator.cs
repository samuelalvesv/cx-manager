using CxManager.Contract.Requests.Feedbacks;
using FluentValidation;

namespace CxManager.Application.Validators.Feedbacks;

public sealed class DeleteFeedbackRequestValidator : AbstractValidator<DeleteFeedbackRequest>
{
    public DeleteFeedbackRequestValidator()
    {
        RuleFor(expression: static x => x.Id)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "O identificador da resposta deve ser positivo.");
    }
}
