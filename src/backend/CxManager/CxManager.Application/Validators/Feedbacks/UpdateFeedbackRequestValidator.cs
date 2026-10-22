using CxManager.Contract.Requests.Feedbacks;
using FluentValidation;

namespace CxManager.Application.Validators.Feedbacks;

public sealed class UpdateFeedbackRequestValidator : AbstractValidator<UpdateFeedbackRequest>
{
    public UpdateFeedbackRequestValidator()
    {
        RuleFor(expression: static x => x.Score)
            .InclusiveBetween(from: 0, to: 10).WithMessage(errorMessage: "A nota deve estar entre 0 e 10.");

        RuleFor(expression: static x => x.Comment)
            .MaximumLength(maximumLength: 1000).WithMessage(errorMessage: "O comentário não pode exceder 1000 caracteres.");
    }
}
