using System.Text.Json.Serialization;
using CxManager.Contract.Responses;
using CxManager.Contract.Responses.Companies;

namespace CxManager.Api.Serialization;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(CompanyResponse))]
[JsonSerializable(typeof(IReadOnlyList<CompanyResponse>))]
public sealed partial class AppJsonSerializerContext : JsonSerializerContext
{
}
