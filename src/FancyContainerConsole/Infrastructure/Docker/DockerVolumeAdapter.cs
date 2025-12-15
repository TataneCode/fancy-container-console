using Docker.DotNet;
using Docker.DotNet.Models;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Domain.Entities;
using FancyContainerConsole.Infrastructure.Mappers;
using System.Runtime.InteropServices;

namespace FancyContainerConsole.Infrastructure.Docker;

public sealed class DockerVolumeAdapter : IVolumeRepository, IDisposable
{
    private readonly DockerClient _client;

    public DockerVolumeAdapter()
    {
        var dockerUri = GetDockerUri();
        _client = new DockerClientConfiguration(dockerUri).CreateClient();
    }

    public async Task<IEnumerable<Volume>> GetVolumesAsync(CancellationToken cancellationToken = default)
    {
        // Get all volumes
        var parameters = new VolumesListParameters();
        var response = await _client.Volumes.ListAsync(parameters, cancellationToken);

        // Get all containers to check which volumes are in use
        var containersListParameters = new ContainersListParameters { All = true };
        var containers = await _client.Containers.ListContainersAsync(containersListParameters, cancellationToken);

        // Build a set of volumes that are in use
        var volumesInUse = new HashSet<string>();
        foreach (var container in containers)
        {
            if (container.Mounts != null)
            {
                foreach (var mount in container.Mounts)
                {
                    if (mount.Type == "volume" && !string.IsNullOrEmpty(mount.Name))
                    {
                        volumesInUse.Add(mount.Name);
                    }
                }
            }
        }

        // Inspect each volume to get full data and check if in use
        var volumes = new List<Volume>();
        foreach (var vol in response.Volumes)
        {
            try
            {
                var inspectedVolume = await _client.Volumes.InspectAsync(vol.Name, cancellationToken);
                var isInUse = volumesInUse.Contains(vol.Name);
                volumes.Add(DockerMapper.ToDomain(inspectedVolume, isInUse));
            }
            catch
            {
                // If inspection fails, use the basic volume data
                var isInUse = volumesInUse.Contains(vol.Name);
                volumes.Add(DockerMapper.ToDomain(vol, isInUse));
            }
        }

        return volumes;
    }

    public async Task<Volume?> GetVolumeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var volume = await _client.Volumes.InspectAsync(name, cancellationToken);

            // Check if volume is in use by any container
            var containersListParameters = new ContainersListParameters { All = true };
            var containers = await _client.Containers.ListContainersAsync(containersListParameters, cancellationToken);

            var isInUse = containers.Any(c =>
                c.Mounts != null &&
                c.Mounts.Any(m => m.Type == "volume" && m.Name == name));

            return DockerMapper.ToDomain(volume, isInUse);
        }
        catch
        {
            return null;
        }
    }

    public async Task DeleteVolumeAsync(string name, CancellationToken cancellationToken = default)
    {
        await _client.Volumes.RemoveAsync(name, false, cancellationToken);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }

    private static Uri GetDockerUri()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new Uri("npipe://./pipe/docker_engine");
        }
        else
        {
            return new Uri("unix:///var/run/docker.sock");
        }
    }
}
