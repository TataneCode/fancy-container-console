using FluentAssertions;
using NetArchTest.Rules;

namespace FancyContainerConsole.Tests.Architecture;

public class ArchitectureTests
{
    private const string DomainNamespace = "FancyContainerConsole.Domain";
    private const string ApplicationNamespace = "FancyContainerConsole.Application";
    private const string InfrastructureNamespace = "FancyContainerConsole.Infrastructure";
    private const string UINamespace = "FancyContainerConsole.UI";

    [Fact]
    public void Domain_ShouldNotHaveDependencyOnOtherLayers()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, UINamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Domain layer should not depend on any other layer");
    }

    [Fact]
    public void Application_ShouldNotHaveDependencyOnInfrastructureOrUI()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(ApplicationNamespace)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, UINamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Application layer should not depend on Infrastructure or UI layers");
    }

    [Fact]
    public void Infrastructure_ShouldNotHaveDependencyOnUI()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .ShouldNot()
            .HaveDependencyOn(UINamespace)
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Infrastructure layer should not depend on UI layer");
    }

    [Fact]
    public void Domain_EntitiesShouldBeSealed()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Domain entities should be sealed");
    }

    [Fact]
    public void Application_ServicesShouldImplementInterfaces()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Services")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Application services should be sealed");
    }

    [Fact]
    public void Infrastructure_ServicesShouldBeSealed()
    {
        var result = Types.InAssembly(typeof(Program).Assembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Docker")
            .And()
            .AreClasses()
            .Should()
            .BeSealed()
            .GetResult();

        result.IsSuccessful.Should().BeTrue("Infrastructure services should be sealed");
    }
}
