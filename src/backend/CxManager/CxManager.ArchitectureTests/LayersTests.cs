using System.Reflection;
using NetArchTest.Rules;
using TestResult = NetArchTest.Rules.TestResult;

namespace CxManager.ArchitectureTests;

public sealed class LayersTests
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
}
