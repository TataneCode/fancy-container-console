using FancyContainerConsole.Domain.Entities;
using FancyContainerConsole.Domain.Enums;
using FancyContainerConsole.Domain.ValueObjects;
using FluentAssertions;

namespace FancyContainerConsole.Tests.Domain;

public class ContainerTests
{
    [Fact]
    public void Constructor_WithValidParameters_ShouldCreateContainer()
    {
        var id = new ContainerId("test123");
        var name = "test-container";
        var image = "nginx:latest";
        var state = ContainerState.Running;
        var createdAt = DateTime.UtcNow;
        var ports = new List<int> { 80, 443 };
        var portMappings = new List<PortMapping> { new PortMapping(80, 8080, "tcp") };
        var networks = new List<NetworkInfo> { new NetworkInfo("bridge", "172.17.0.2") };
        var memoryUsage = 1024L;
        var volumes = new List<string> { "vol1" };

        var container = new Container(id, name, image, state, createdAt, ports, portMappings, networks, memoryUsage, volumes);

        container.Id.Should().Be(id);
        container.Name.Should().Be(name);
        container.Image.Should().Be(image);
        container.State.Should().Be(state);
        container.CreatedAt.Should().Be(createdAt);
        container.Ports.Should().BeEquivalentTo(ports);
    }

    [Fact]
    public void Constructor_WithNullId_ShouldThrowArgumentNullException()
    {
        var action = () => new Container(
            null!,
            "name",
            "image",
            ContainerState.Running,
            DateTime.UtcNow,
            Array.Empty<int>(),
            Array.Empty<PortMapping>(),
            Array.Empty<NetworkInfo>(),
            0L,
            Array.Empty<string>()
        );

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IsRunning_WhenStateIsRunning_ShouldReturnTrue()
    {
        var container = CreateTestContainer(ContainerState.Running);

        container.IsRunning().Should().BeTrue();
    }

    [Fact]
    public void IsRunning_WhenStateIsNotRunning_ShouldReturnFalse()
    {
        var container = CreateTestContainer(ContainerState.Stopped);

        container.IsRunning().Should().BeFalse();
    }

    [Theory]
    [InlineData(ContainerState.Stopped, true)]
    [InlineData(ContainerState.Exited, true)]
    [InlineData(ContainerState.Created, true)]
    [InlineData(ContainerState.Running, false)]
    [InlineData(ContainerState.Paused, false)]
    public void CanBeStarted_ShouldReturnCorrectValue(ContainerState state, bool expected)
    {
        var container = CreateTestContainer(state);

        container.CanBeStarted().Should().Be(expected);
    }

    [Theory]
    [InlineData(ContainerState.Running, true)]
    [InlineData(ContainerState.Paused, true)]
    [InlineData(ContainerState.Restarting, true)]
    [InlineData(ContainerState.Stopped, false)]
    [InlineData(ContainerState.Exited, false)]
    public void CanBeStopped_ShouldReturnCorrectValue(ContainerState state, bool expected)
    {
        var container = CreateTestContainer(state);

        container.CanBeStopped().Should().Be(expected);
    }

    private static Container CreateTestContainer(ContainerState state)
    {
        return new Container(
            new ContainerId("test123"),
            "test-container",
            "nginx:latest",
            state,
            DateTime.UtcNow,
            Array.Empty<int>(),
            Array.Empty<PortMapping>(),
            Array.Empty<NetworkInfo>(),
            0L,
            Array.Empty<string>()
        );
    }
}
