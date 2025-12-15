namespace FancyContainerConsole.Application.DTOs;

public sealed record ImageDto(
    string Id,
    string Repository,
    string Tag,
    long Size,
    DateTime CreatedAt,
    bool InUse);
