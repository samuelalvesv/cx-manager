using CxManager.Contract.Requests.Contacts;
using FluentValidation;

namespace CxManager.Application.Validators.Contacts;

public sealed class GetContactByIdRequestValidator : AbstractValidator<GetContactByIdRequest>
{
    public GetContactByIdRequestValidator()
    {
        RuleFor(expression: static x => x.Id)
            .GreaterThan(valueToCompare: 0)
            .WithMessage(errorMessage: "O identificador do contato deve ser maior que zero.");
    }
}
