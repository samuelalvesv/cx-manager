using CxManager.Contract.Requests.Companies;
using FluentValidation;

namespace CxManager.Application.Validators.Companies;

public sealed class GetCompanyByIdRequestValidator : AbstractValidator<GetCompanyByIdRequest>
{
    public GetCompanyByIdRequestValidator()
    {
        RuleFor(expression: static x => x.Id)
            .GreaterThan(valueToCompare: 0)
            .WithMessage(errorMessage: "O identificador da empresa deve ser maior que zero.");
    }
}
