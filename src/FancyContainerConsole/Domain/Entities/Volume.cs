using FancyContainerConsole.Domain.ValueObjects;

namespace FancyContainerConsole.Domain.Entities;

public sealed class Volume
{
    public VolumeId Id { get; }
    public string Name { get; }
    public long Size { get; }
    public bool InUse { get; }
    public DateTime CreatedAt { get; }

    public Volume(
        VolumeId id,
        string name,
        long size,
        bool inUse,
        DateTime createdAt)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Size = size;
        InUse = inUse;
        CreatedAt = createdAt;
    }

    public bool CanBeDeleted() => !InUse;
}
