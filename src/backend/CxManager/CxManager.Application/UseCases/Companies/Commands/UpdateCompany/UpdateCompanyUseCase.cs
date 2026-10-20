using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Companies;
using CxManager.Contract.Responses.Companies;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;
using FluentValidation;
using FluentValidation.Results;
using ValidationException = CxManager.Domain.Exceptions.ValidationException;

namespace CxManager.Application.UseCases.Companies.Commands.UpdateCompany;

public sealed class UpdateCompanyUseCase(
    ICompanyRepository companyRepository,
    IValidator<UpdateCompanyRequest> validator) : IUpdateCompanyUseCase
{
    public async Task<CompanyResponse> ExecuteAsync(
        long id,
        UpdateCompanyRequest request,
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await validator.ValidateAsync(
            instance: request,
            cancellation: cancellationToken);

        if (!validationResult.IsValid)
            throw new ValidationException(message: validationResult.Errors[0].ErrorMessage);

        Company? company = await companyRepository.GetByIdAsync(
            id: id,
            cancellationToken: cancellationToken);

        if (company is null || company.IsDeleted)
            throw new EntityNotFoundException(message: "Empresa não encontrada.");

        company.Update(
            name: request.Name,
            description: request.Description);

        await companyRepository.UpdateAsync(
            company: company,
            cancellationToken: cancellationToken);

        return CompanyMapper.ToResponse(company: company);
    }
}
