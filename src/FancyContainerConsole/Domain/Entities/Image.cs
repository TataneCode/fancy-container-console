using FancyContainerConsole.Domain.ValueObjects;

namespace FancyContainerConsole.Domain.Entities;

public sealed class Image
{
    public ImageId Id { get; }
    public string Repository { get; }
    public string Tag { get; }
    public long Size { get; }
    public DateTime CreatedAt { get; }
    public bool InUse { get; }

    public Image(
        ImageId id,
        string repository,
        string tag,
        long size,
        DateTime createdAt,
        bool inUse)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Repository = repository ?? throw new ArgumentNullException(nameof(repository));
        Tag = tag ?? throw new ArgumentNullException(nameof(tag));
        Size = size;
        CreatedAt = createdAt;
        InUse = inUse;
    }

    public bool CanBeDeleted() => !InUse;

    public string FullName => $"{Repository}:{Tag}";
}
