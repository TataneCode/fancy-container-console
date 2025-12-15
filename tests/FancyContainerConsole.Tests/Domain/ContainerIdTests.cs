using FancyContainerConsole.Domain.ValueObjects;
using FluentAssertions;

namespace FancyContainerConsole.Tests.Domain;

public class ContainerIdTests
{
    [Fact]
    public void Constructor_WithValidValue_ShouldCreateContainerId()
    {
        var value = "test123";

        var containerId = new ContainerId(value);

        containerId.Value.Should().Be(value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidValue_ShouldThrowArgumentException(string? value)
    {
        var action = () => new ContainerId(value!);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        var value = "test123";
        var containerId = new ContainerId(value);

        containerId.ToString().Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_ShouldConvertToString()
    {
        var value = "test123";
        var containerId = new ContainerId(value);

        string result = containerId;

        result.Should().Be(value);
    }
}
