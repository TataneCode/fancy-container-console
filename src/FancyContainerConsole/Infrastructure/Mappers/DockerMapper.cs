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
        var ports = dockerContainer.Ports?.Select(p => (int)p.PublicPort).Where(p => p > 0).ToList() ?? new List<int>();

        var portMappings = dockerContainer.Ports?
            .Where(p => p.PublicPort > 0)
            .Select(p => new PortMapping((int)p.PrivatePort, (int)p.PublicPort, p.Type ?? "tcp"))
            .ToList() ?? new List<PortMapping>();

        var networks = dockerContainer.NetworkSettings?.Networks?
            .Select(n => new NetworkInfo(n.Key, n.Value.IPAddress ?? "N/A"))
            .ToList() ?? new List<NetworkInfo>();

        // SizeRootFs is the total filesystem size (in bytes)
        var memoryUsage = dockerContainer.SizeRootFs;

        var volumes = dockerContainer.Mounts?
            .Select(m => m.Name ?? m.Source ?? "Unknown")
            .ToList() ?? new List<string>();

        return new Container(containerId, name, image, state, createdAt, ports, portMappings, networks, memoryUsage, volumes);
    }

    public static Volume ToDomain(VolumeResponse dockerVolume, bool? isInUse = null)
    {
        ArgumentNullException.ThrowIfNull(dockerVolume);

        var volumeId = new VolumeId(dockerVolume.Name);
        var name = dockerVolume.Name;

        // Try to get size from UsageData
        long size = dockerVolume.UsageData?.Size ?? 0L;

        // Use provided isInUse value if available, otherwise try to get from UsageData
        bool inUseValue;
        if (isInUse.HasValue)
        {
            inUseValue = isInUse.Value;
        }
        else
        {
            inUseValue = dockerVolume.UsageData?.RefCount > 0;
        }

        var createdAt = DateTime.TryParse(dockerVolume.CreatedAt, out var dt) ? dt : DateTime.MinValue;

        return new Volume(volumeId, name, size, inUseValue, createdAt);
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
