using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Application.Services;
using FancyContainerConsole.Domain.Entities;
using FancyContainerConsole.Domain.Enums;
using FancyContainerConsole.Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace FancyContainerConsole.Tests.Application;

public class ContainerServiceTests
{
    private readonly Mock<IContainerRepository> _containerRepositoryMock;
    private readonly ContainerService _sut;

    public ContainerServiceTests()
    {
        _containerRepositoryMock = new Mock<IContainerRepository>();
        _sut = new ContainerService(_containerRepositoryMock.Object);
    }

    [Fact]
    public async Task GetAllContainersAsync_ShouldReturnMappedContainers()
    {
        var containers = new List<Container>
        {
            new(new ContainerId("1"), "container1", "image1", ContainerState.Running, DateTime.UtcNow, Array.Empty<int>(), Array.Empty<PortMapping>(), Array.Empty<NetworkInfo>(), 0L, Array.Empty<string>()),
            new(new ContainerId("2"), "container2", "image2", ContainerState.Stopped, DateTime.UtcNow, Array.Empty<int>(), Array.Empty<PortMapping>(), Array.Empty<NetworkInfo>(), 0L, Array.Empty<string>())
        };

        _containerRepositoryMock.Setup(x => x.GetContainersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(containers);

        var result = await _sut.GetAllContainersAsync();

        result.Should().HaveCount(2);
        result.First().Name.Should().Be("container1");
        result.Last().Name.Should().Be("container2");
    }

    [Fact]
    public async Task GetContainerLogsAsync_ShouldReturnLogs()
    {
        var expectedLogs = "Test logs";
        _containerRepositoryMock.Setup(x => x.GetLogsAsync("test123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedLogs);

        var result = await _sut.GetContainerLogsAsync("test123");

        result.Should().Be(expectedLogs);
    }

    [Fact]
    public async Task StartContainerAsync_ShouldCallRepository()
    {
        await _sut.StartContainerAsync("test123");

        _containerRepositoryMock.Verify(x => x.StartContainerAsync("test123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task StopContainerAsync_ShouldCallRepository()
    {
        await _sut.StopContainerAsync("test123");

        _containerRepositoryMock.Verify(x => x.StopContainerAsync("test123", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteContainerAsync_ShouldCallRepository()
    {
        await _sut.DeleteContainerAsync("test123");

        _containerRepositoryMock.Verify(x => x.DeleteContainerAsync("test123", It.IsAny<CancellationToken>()), Times.Once);
    }
}
