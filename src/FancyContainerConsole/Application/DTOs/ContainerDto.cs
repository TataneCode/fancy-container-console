namespace FancyContainerConsole.Application.DTOs;

public sealed record ContainerDto(
    string Id,
    string Name,
    string Image,
    string State,
    DateTime CreatedAt,
    IReadOnlyList<int> Ports);
