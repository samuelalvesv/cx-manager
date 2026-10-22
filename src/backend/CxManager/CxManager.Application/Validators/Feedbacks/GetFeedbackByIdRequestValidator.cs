using CxManager.Contract.Requests.Feedbacks;
using FluentValidation;

namespace CxManager.Application.Validators.Feedbacks;

public sealed class GetFeedbackByIdRequestValidator : AbstractValidator<GetFeedbackByIdRequest>
{
    public GetFeedbackByIdRequestValidator()
    {
        RuleFor(expression: static x => x.Id)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "O identificador da resposta deve ser positivo.");
    }
}
