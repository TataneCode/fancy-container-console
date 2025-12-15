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
        var parameters = new VolumesListParameters();
        var response = await _client.Volumes.ListAsync(parameters, cancellationToken);
        return response.Volumes.Select(DockerMapper.ToDomain);
    }

    public async Task<Volume?> GetVolumeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var volume = await _client.Volumes.InspectAsync(name, cancellationToken);
            return DockerMapper.ToDomain(volume);
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
