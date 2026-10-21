using CxManager.Contract.Requests.Contacts;
using FluentValidation;

namespace CxManager.Application.Validators.Contacts;

public sealed class UpdateContactRequestValidator : AbstractValidator<UpdateContactRequest>
{
    public UpdateContactRequestValidator()
    {
        RuleFor(expression: static x => x.Name)
            .NotEmpty().WithMessage(errorMessage: "O nome do contato é obrigatório.")
            .Length(min: 2, max: 100).WithMessage(errorMessage: "O nome do contato deve ter entre 2 e 100 caracteres.");

        RuleFor(expression: static x => x.Email)
            .NotEmpty().WithMessage(errorMessage: "O e-mail é obrigatório.")
            .Length(min: 5, max: 150).WithMessage(errorMessage: "O e-mail deve ter entre 5 e 150 caracteres.")
            .EmailAddress().WithMessage(errorMessage: "O e-mail informado possui formato inválido.");

        RuleFor(expression: static x => x.Segment)
            .NotEmpty().WithMessage(errorMessage: "O segmento é obrigatório.")
            .Length(min: 2, max: 50).WithMessage(errorMessage: "O segmento deve ter entre 2 e 50 caracteres.");
    }
}
