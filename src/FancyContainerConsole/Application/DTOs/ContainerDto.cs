namespace FancyContainerConsole.Application.DTOs;

public sealed record PortMappingDto(int PrivatePort, int PublicPort, string Type);

public sealed record NetworkInfoDto(string Name, string IpAddress);

public sealed record ContainerDto(
    string Id,
    string Name,
    string Image,
    string State,
    DateTime CreatedAt,
    IReadOnlyList<int> Ports,
    IReadOnlyList<PortMappingDto> PortMappings,
    IReadOnlyList<NetworkInfoDto> Networks,
    long MemoryUsage,
    IReadOnlyList<string> Volumes);
