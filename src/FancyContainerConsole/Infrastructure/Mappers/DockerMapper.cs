using Docker.DotNet.Models;
using FancyContainerConsole.Domain.Entities;
using FancyContainerConsole.Domain.Enums;
using FancyContainerConsole.Domain.ValueObjects;

namespace FancyContainerConsole.Infrastructure.Mappers;

public static class DockerMapper
{
    public static Container ToDomain(ContainerListResponse dockerContainer)
    {
        ArgumentNullException.ThrowIfNull(dockerContainer);

        var containerId = new ContainerId(dockerContainer.ID);
        var name = dockerContainer.Names?.FirstOrDefault()?.TrimStart('/') ?? "Unknown";
        var image = dockerContainer.Image ?? "Unknown";
        var state = MapState(dockerContainer.State);
        var createdAt = dockerContainer.Created;
        var ports = dockerContainer.Ports?.Select(p => (int)p.PublicPort).ToList() ?? new List<int>();

        return new Container(containerId, name, image, state, createdAt, ports);
    }

    private static Domain.Enums.ContainerState MapState(string state)
    {
        return state?.ToLowerInvariant() switch
        {
            "running" => Domain.Enums.ContainerState.Running,
            "paused" => Domain.Enums.ContainerState.Paused,
            "stopped" => Domain.Enums.ContainerState.Stopped,
            "exited" => Domain.Enums.ContainerState.Exited,
            "dead" => Domain.Enums.ContainerState.Dead,
            "created" => Domain.Enums.ContainerState.Created,
            "removing" => Domain.Enums.ContainerState.Removing,
            "restarting" => Domain.Enums.ContainerState.Restarting,
            _ => Domain.Enums.ContainerState.Stopped
        };
    }
}
