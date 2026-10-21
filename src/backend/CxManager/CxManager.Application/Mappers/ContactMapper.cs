using CxManager.Contract.Responses.Contacts;
using CxManager.Domain.Entities;
using Riok.Mapperly.Abstractions;

namespace CxManager.Application.Mappers;

[Mapper]
public static partial class ContactMapper
{
    [MapperIgnoreSource(nameof(Contact.CompanyId))]
    [MapperIgnoreSource(nameof(Contact.DeletedAt))]
    [MapperIgnoreSource(nameof(Contact.IsDeleted))]
    public static partial ContactResponse ToResponse(Contact contact);

    public static partial IReadOnlyList<ContactResponse> ToResponseList(IReadOnlyList<Contact> contacts);
}
