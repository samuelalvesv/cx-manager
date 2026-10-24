using CxManager.Application.Mappers;
using CxManager.Contract.Responses.Companies;
using CxManager.Domain.Entities;
using CxManager.Domain.Exceptions;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Companies.Queries.GetCompanyImpact;

public sealed class GetCompanyImpactUseCase(
    ICompanyRepository companyRepository,
    IContactRepository contactRepository,
    ISurveyRepository surveyRepository) : IGetCompanyImpactUseCase
{
    public async Task<CompanyImpactSummaryResponse> ExecuteAsync(long companyId, CancellationToken cancellationToken = default)
    {
        Company? company = await companyRepository.GetByIdAsync(
            id: companyId,
            cancellationToken: cancellationToken);

        if (company is null || company.IsDeleted)
            throw new EntityNotFoundException(message: "Empresa não encontrada.");

        int activeContactsCount = await contactRepository.GetActiveCountByCompanyAsync(
            companyId: companyId,
            cancellationToken: cancellationToken);

        IReadOnlyList<Survey> surveys = await surveyRepository.GetAllActiveByCompanyAsync(
            companyId: companyId,
            cancellationToken: cancellationToken);

        IReadOnlyDictionary<long, int> counts = await surveyRepository.GetActiveResponsesCountsByCompanyAsync(
            companyId: companyId,
            cancellationToken: cancellationToken);

        var surveyImpacts = surveys.Select(s => new CompanySurveyImpactItemResponse
        {
            Id = s.Id,
            Name = s.Name,
            Type = SurveyMapper.MapSurveyTypeToString(type: s.Type),
            ResponsesCount = counts.GetValueOrDefault(key: s.Id, defaultValue: 0)
        }).ToList();

        int totalResponsesCount = surveyImpacts.Sum(static s => s.ResponsesCount);

        return new CompanyImpactSummaryResponse
        {
            CompanyId = company.Id,
            CompanyName = company.Name,
            ActiveContactsCount = activeContactsCount,
            TotalResponsesCount = totalResponsesCount,
            Surveys = surveyImpacts
        };
    }
}
