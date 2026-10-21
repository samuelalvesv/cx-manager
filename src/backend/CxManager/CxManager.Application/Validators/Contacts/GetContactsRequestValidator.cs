using CxManager.Application.Validators.Common;
using CxManager.Contract.Requests.Contacts;
using FluentValidation;

namespace CxManager.Application.Validators.Contacts;

public sealed class GetContactsRequestValidator : PagedRequestValidator<GetContactsRequest>
{
    public GetContactsRequestValidator()
    {
        RuleFor(expression: static x => x.Search)
            .MaximumLength(maximumLength: 100)
            .WithMessage(errorMessage: "O termo de busca não pode exceder 100 caracteres.")
            .When(predicate: static x => !string.IsNullOrEmpty(x.Search));
    }
}
