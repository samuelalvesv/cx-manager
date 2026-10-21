using CxManager.Contract;
using CxManager.Contract.Requests;
using FluentValidation;

namespace CxManager.Application.Validators.Common;

public abstract class PagedRequestValidator<T> : AbstractValidator<T> where T : PagedRequest
{
    protected PagedRequestValidator()
    {
        RuleFor(expression: static x => x.Page)
            .GreaterThanOrEqualTo(valueToCompare: 1)
            .WithMessage(errorMessage: "O número da página deve ser maior ou igual a 1.");

        RuleFor(expression: static x => x.PageSize)
            .InclusiveBetween(from: 1, to: ContractDefaults.MaxPageSize)
            .WithMessage(errorMessage: $"O tamanho da página deve estar entre 1 e {ContractDefaults.MaxPageSize}.");
    }
}
