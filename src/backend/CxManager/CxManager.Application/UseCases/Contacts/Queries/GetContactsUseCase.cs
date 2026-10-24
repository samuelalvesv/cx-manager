using CxManager.Application.Interfaces;
using CxManager.Application.Mappers;
using CxManager.Contract.Requests.Contacts;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Contacts;
using CxManager.Domain.Entities;
using CxManager.Domain.Interfaces;

namespace CxManager.Application.UseCases.Contacts.Queries;

public sealed class GetContactsUseCase(
    IContactRepository contactRepository,
    ICompanyContext companyContext) : IGetContactsUseCase
{
    public async Task<PagedResponse<ContactResponse>> ExecuteAsync(
        GetContactsRequest request,
        CancellationToken cancellationToken = default)
    {
        long companyId = companyContext.CompanyId;

        (IReadOnlyList<Contact> items, int total) = await contactRepository.GetPagedByCompanyAsync(
            companyId: companyId,
            page: request.Page,
            pageSize: request.PageSize,
            search: request.Search,
            sortBy: request.SortBy,
            sortDir: request.SortDir,
            cancellationToken: cancellationToken);

        IReadOnlyList<ContactResponse> mappedItems = ContactMapper.ToResponseList(contacts: items);

        return new PagedResponse<ContactResponse>
        {
            Items = mappedItems,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }
}
