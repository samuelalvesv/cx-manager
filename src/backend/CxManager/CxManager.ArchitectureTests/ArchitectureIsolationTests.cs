using System.Reflection;
using CxManager.Api.Middleware;
using CxManager.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NetArchTest.Rules;
using TestResult = NetArchTest.Rules.TestResult;

namespace CxManager.ArchitectureTests;

public sealed class ArchitectureIsolationTests
{
    private const string DomainNamespace = "CxManager.Domain";
    private const string ContractNamespace = "CxManager.Contract";
    private const string ApplicationNamespace = "CxManager.Application";
    private const string InfrastructureNamespace = "CxManager.Infrastructure";
    private const string ApiNamespace = "CxManager.Api";

    private static readonly Assembly DomainAssembly = Assembly.Load(assemblyString: DomainNamespace);
    private static readonly Assembly ContractAssembly = Assembly.Load(assemblyString: ContractNamespace);
    private static readonly Assembly ApplicationAssembly = Assembly.Load(assemblyString: ApplicationNamespace);
    private static readonly Assembly InfrastructureAssembly = Assembly.Load(assemblyString: InfrastructureNamespace);

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherLayers()
    {
        TestResult result = Types.InAssembly(assembly: DomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(dependencies: [ApplicationNamespace, InfrastructureNamespace, ApiNamespace, ContractNamespace])
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "A camada Domain não deve depender de outras camadas.");
    }

    [Fact]
    public void Contract_ShouldNotHaveDependencyOnOtherLayers()
    {
        TestResult result = Types.InAssembly(assembly: ContractAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(dependencies: [DomainNamespace, ApplicationNamespace, InfrastructureNamespace, ApiNamespace])
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "A camada Contract não deve depender de outras camadas.");
    }

    [Fact]
    public void Contract_ShouldNotDependOnSystemComponentModelDataAnnotations()
    {
        TestResult result = Types.InAssembly(assembly: ContractAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(dependencies: "System.ComponentModel.DataAnnotations")
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "A camada Contract não deve depender de DataAnnotations.");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnOuterLayers()
    {
        TestResult result = Types.InAssembly(assembly: ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(dependencies: [InfrastructureNamespace, ApiNamespace])
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "A camada Application não deve depender de Infrastructure ou Api.");
    }

    [Fact]
    public void Application_ShouldNotDependOnReflectionBasedScanning()
    {
        TestResult result = Types.InAssembly(assembly: ApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(dependencies: "FluentValidation.DependencyInjectionExtensions")
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "A camada Application não deve depender de pacotes de scanning reflexivo.");
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnContractOrApi()
    {
        TestResult result = Types.InAssembly(assembly: InfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(dependencies: [ContractNamespace, ApiNamespace])
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "A camada Infrastructure não deve depender de Contract ou Api.");
    }

    [Fact]
    public void Interfaces_InAllLayers_ShouldStartWithPrefixI()
    {
        TestResult result = Types.InAssemblies(assemblies: [DomainAssembly, ApplicationAssembly, InfrastructureAssembly])
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "Todas as interfaces devem começar com o prefixo 'I'.");
    }

    [Fact]
    public void ConcreteClasses_InAllLayers_ShouldBeSealed()
    {
        TestResult result = Types.InAssemblies(assemblies: [DomainAssembly, ContractAssembly, ApplicationAssembly, InfrastructureAssembly])
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .AreNotStatic()
            .Should()
            .BeSealed()
            .GetResult();

        Assert.True(condition: result.IsSuccessful, userMessage: "Todas as classes concretas devem ser marcadas como sealed.");
    }

    [Fact]
    public void Contract_Dtos_ShouldBeRecords()
    {
        var contractTypes = ContractAssembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !(t.IsAbstract && t.IsSealed))
            .ToList();

        Assert.NotEmpty(collection: contractTypes);

        foreach (Type type in contractTypes)
        {
            PropertyInfo? equalityContract = type.GetProperty(
                name: "EqualityContract",
                bindingAttr: BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            Assert.True(
                condition: equalityContract is not null,
                userMessage: $"O tipo de contrato '{type.Name}' deve ser um record imutável (sealed record).");
        }
    }

    [Fact]
    public async Task GlobalExceptionHandler_ShouldExplicitlyHandleAllDomainExceptions()
    {
        GlobalExceptionHandler handler = new(logger: NullLogger<GlobalExceptionHandler>.Instance);

        var domainExceptionTypes = DomainAssembly.GetTypes()
            .Where(t => typeof(DomainException).IsAssignableFrom(t) && !t.IsAbstract)
            .ToList();

        Assert.NotEmpty(collection: domainExceptionTypes);

        foreach (Type exceptionType in domainExceptionTypes)
        {
            var exception = (DomainException)Activator.CreateInstance(
                type: exceptionType,
                args: ["Mensagem de teste de arquitetura"])!;

            DefaultHttpContext httpContext = new();
            MemoryStream stream = new();
            httpContext.Response.Body = stream;

            bool handled = await handler.TryHandleAsync(
                httpContext: httpContext,
                exception: exception,
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.True(condition: handled, userMessage: $"O handler não tratou a exceção {exceptionType.Name}.");
            Assert.NotEqual(
                expected: StatusCodes.Status500InternalServerError,
                actual: httpContext.Response.StatusCode);
        }
    }
}
