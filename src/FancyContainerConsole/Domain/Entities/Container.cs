using FancyContainerConsole.Domain.Enums;
using FancyContainerConsole.Domain.ValueObjects;

namespace FancyContainerConsole.Domain.Entities;

public sealed class Container
{
    public ContainerId Id { get; }
    public string Name { get; }
    public string Image { get; }
    public ContainerState State { get; }
    public DateTime CreatedAt { get; }
    public IReadOnlyList<int> Ports { get; }

    public Container(
        ContainerId id,
        string name,
        string image,
        ContainerState state,
        DateTime createdAt,
        IReadOnlyList<int> ports)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Image = image ?? throw new ArgumentNullException(nameof(image));
        State = state;
        CreatedAt = createdAt;
        Ports = ports ?? Array.Empty<int>();
    }

    public bool IsRunning() => State == ContainerState.Running;
    public bool CanBeStarted() => State is ContainerState.Stopped or ContainerState.Exited or ContainerState.Created;
    public bool CanBeStopped() => State is ContainerState.Running or ContainerState.Paused or ContainerState.Restarting;
}
