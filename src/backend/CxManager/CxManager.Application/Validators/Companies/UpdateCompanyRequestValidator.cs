using CxManager.Contract.Requests.Companies;
using FluentValidation;

namespace CxManager.Application.Validators.Companies;

public sealed class UpdateCompanyRequestValidator : AbstractValidator<UpdateCompanyRequest>
{
    public UpdateCompanyRequestValidator()
    {
        RuleFor(expression: static x => x.Name)
            .NotEmpty().WithMessage(errorMessage: "O nome da empresa é obrigatório.")
            .Length(min: 2, max: 100).WithMessage(errorMessage: "O nome da empresa deve ter entre 2 e 100 caracteres.");

        RuleFor(expression: static x => x.Description)
            .NotEmpty().WithMessage(errorMessage: "A descrição da empresa é obrigatória.")
            .Length(min: 2, max: 500).WithMessage(errorMessage: "A descrição da empresa deve ter entre 2 e 500 caracteres.");
    }
}
