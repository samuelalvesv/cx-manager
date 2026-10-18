using System.Text.Json.Serialization;

namespace CxManager.Infrastructure.Persistence.Seed;

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]

[JsonSerializable(typeof(SeedData))]
internal sealed partial class SeedJsonSerializerContext : JsonSerializerContext
{
}
