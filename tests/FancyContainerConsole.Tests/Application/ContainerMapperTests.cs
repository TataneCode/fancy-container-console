using FancyContainerConsole.Application.Mappers;
using FancyContainerConsole.Domain.Entities;
using FancyContainerConsole.Domain.Enums;
using FancyContainerConsole.Domain.ValueObjects;
using FluentAssertions;

namespace FancyContainerConsole.Tests.Application;

public class ContainerMapperTests
{
    [Fact]
    public void ToDto_WithValidContainer_ShouldMapCorrectly()
    {
        var container = new Container(
            new ContainerId("test123"),
            "test-container",
            "nginx:latest",
            ContainerState.Running,
            new DateTime(2024, 1, 1, 12, 0, 0),
            new List<int> { 80, 443 },
            Array.Empty<PortMapping>(),
            Array.Empty<NetworkInfo>(),
            0L,
            Array.Empty<string>()
        );

        var dto = ContainerMapper.ToDto(container);

        dto.Id.Should().Be("test123");
        dto.Name.Should().Be("test-container");
        dto.Image.Should().Be("nginx:latest");
        dto.State.Should().Be("Running");
        dto.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0));
        dto.Ports.Should().BeEquivalentTo(new[] { 80, 443 });
    }

    [Fact]
    public void ToDto_WithNullContainer_ShouldThrowArgumentNullException()
    {
        Container nullContainer = null!;
        var action = () => ContainerMapper.ToDto(nullContainer);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToDto_WithMultipleContainers_ShouldMapAll()
    {
        var containers = new List<Container>
        {
            new(new ContainerId("1"), "container1", "image1", ContainerState.Running, DateTime.UtcNow, Array.Empty<int>(), Array.Empty<PortMapping>(), Array.Empty<NetworkInfo>(), 0L, Array.Empty<string>()),
            new(new ContainerId("2"), "container2", "image2", ContainerState.Stopped, DateTime.UtcNow, Array.Empty<int>(), Array.Empty<PortMapping>(), Array.Empty<NetworkInfo>(), 0L, Array.Empty<string>())
        };

        var dtos = ContainerMapper.ToDto(containers).ToList();

        dtos.Should().HaveCount(2);
        dtos[0].Name.Should().Be("container1");
        dtos[1].Name.Should().Be("container2");
    }
}
