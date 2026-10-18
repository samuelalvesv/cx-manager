using System.Reflection;
using System.Text.Json;

namespace CxManager.IntegrationTests;

public sealed class SeedResourceTests
{
    [Fact]
    public void SeedResource_ShouldBeEmbeddedInInfrastructureAssembly()
    {
        Assembly infrastructureAssembly = Infrastructure.InfrastructureMarker.Assembly;
        const string resourceName = "CxManager.Infrastructure.Data.seed.json";

        using Stream? stream = infrastructureAssembly.GetManifestResourceStream(name: resourceName);

        Assert.True(condition: stream is not null, userMessage: $"O recurso embutido '{resourceName}' não foi encontrado no assembly de infraestrutura.");
        Assert.True(condition: stream.Length > 0, userMessage: "O arquivo seed.json embutido está vazio.");

        using var document = JsonDocument.Parse(utf8Json: stream);
        JsonElement root = document.RootElement;

        Assert.True(condition: root.TryGetProperty(propertyName: "company", value: out JsonElement company), userMessage: "Propriedade 'company' ausente no seed.json.");
        Assert.Equal(expected: "Vita Bem-Estar", actual: company.GetProperty(propertyName: "name").GetString());

        Assert.True(condition: root.TryGetProperty(propertyName: "surveys", value: out JsonElement surveys), userMessage: "Propriedade 'surveys' ausente no seed.json.");
        Assert.Equal(expected: 3, actual: surveys.GetArrayLength());

        Assert.True(condition: root.TryGetProperty(propertyName: "contacts", value: out JsonElement contacts), userMessage: "Propriedade 'contacts' ausente no seed.json.");
        Assert.Equal(expected: 320, actual: contacts.GetArrayLength());

        Assert.True(condition: root.TryGetProperty(propertyName: "responses", value: out JsonElement responses), userMessage: "Propriedade 'responses' ausente no seed.json.");
        Assert.Equal(expected: 1284, actual: responses.GetArrayLength());

        int activeResponses = responses.EnumerateArray()
            .Count(predicate: r => r.GetProperty(propertyName: "deletedAt").ValueKind == JsonValueKind.Null);
        Assert.Equal(expected: 1246, actual: activeResponses);

        int deletedResponses = responses.EnumerateArray()
            .Count(predicate: r => r.GetProperty(propertyName: "deletedAt").ValueKind != JsonValueKind.Null);
        Assert.Equal(expected: 38, actual: deletedResponses);
    }
}
