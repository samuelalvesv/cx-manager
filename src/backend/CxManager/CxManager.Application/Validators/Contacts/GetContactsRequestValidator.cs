using CxManager.Application.Validators.Common;
using CxManager.Contract.Requests.Contacts;
using FluentValidation;

namespace CxManager.Application.Validators.Contacts;

public sealed class GetContactsRequestValidator : PagedRequestValidator<GetContactsRequest>
{
    private static readonly string[] AllowedSortFields = ["name", "email", "segment", "createdAt"];
    private static readonly string[] AllowedSortDirections = ["asc", "desc"];

    public GetContactsRequestValidator()
    {
        RuleFor(expression: static x => x.Search)
            .MaximumLength(maximumLength: 100)
            .WithMessage(errorMessage: "O termo de busca não pode exceder 100 caracteres.")
            .When(predicate: static x => !string.IsNullOrEmpty(x.Search));

        RuleFor(expression: static x => x.SortBy)
            .Must(predicate: static sortBy => AllowedSortFields.Contains(value: sortBy, comparer: StringComparer.Ordinal))
            .WithMessage(errorMessage: "O campo de ordenação deve ser 'name', 'email', 'segment' ou 'createdAt'.")
            .When(predicate: static x => x.SortBy is not null);

        RuleFor(expression: static x => x.SortDir)
            .Must(predicate: static sortDir => AllowedSortDirections.Contains(value: sortDir, comparer: StringComparer.Ordinal))
            .WithMessage(errorMessage: "A direção de ordenação deve ser 'asc' ou 'desc'.")
            .When(predicate: static x => x.SortDir is not null);
    }
}
