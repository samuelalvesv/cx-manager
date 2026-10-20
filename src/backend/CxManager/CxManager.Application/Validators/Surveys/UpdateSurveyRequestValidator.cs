using CxManager.Contract.Requests.Surveys;
using FluentValidation;

namespace CxManager.Application.Validators.Surveys;

public sealed class UpdateSurveyRequestValidator : AbstractValidator<UpdateSurveyRequest>
{
    public UpdateSurveyRequestValidator()
    {
        RuleFor(expression: static x => x.Name)
            .NotEmpty().WithMessage(errorMessage: "O nome da pesquisa é obrigatório.")
            .Length(min: 2, max: 100).WithMessage(errorMessage: "O nome da pesquisa deve ter entre 2 e 100 caracteres.");
    }
}
