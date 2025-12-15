namespace FancyContainerConsole.Infrastructure.DTOs;

public sealed record DockerContainerDto(
    string Id,
    string Name,
    string Image,
    string State,
    long Created,
    IReadOnlyList<int> Ports);
