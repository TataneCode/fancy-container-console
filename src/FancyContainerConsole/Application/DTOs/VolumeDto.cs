namespace FancyContainerConsole.Application.DTOs;

public sealed record VolumeDto(
    string Id,
    string Name,
    long Size,
    bool InUse,
    DateTime CreatedAt);
