using CxManager.Contract.Requests.Feedbacks;
using FluentValidation;

namespace CxManager.Application.Validators.Feedbacks;

public sealed class GetContactFeedbacksRequestValidator : AbstractValidator<GetContactFeedbacksRequest>
{
    public GetContactFeedbacksRequestValidator()
    {
        RuleFor(expression: static x => x.Id)
            .GreaterThan(valueToCompare: 0).WithMessage(errorMessage: "O identificador do contato deve ser positivo.");
    }
}
