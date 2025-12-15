using Docker.DotNet;
using Docker.DotNet.Models;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Domain.Entities;
using FancyContainerConsole.Infrastructure.Mappers;
using System.Runtime.InteropServices;
using System.Text;

namespace FancyContainerConsole.Infrastructure.Docker;

public sealed class DockerClientAdapter : IContainerRepository, IDisposable
{
    private readonly DockerClient _client;

    public DockerClientAdapter()
    {
        var dockerUri = GetDockerUri();
        _client = new DockerClientConfiguration(dockerUri).CreateClient();
    }

    public async Task<IEnumerable<Container>> GetContainersAsync(CancellationToken cancellationToken = default)
    {
        var parameters = new ContainersListParameters { All = true };
        var containers = await _client.Containers.ListContainersAsync(parameters, cancellationToken);
        return containers.Select(DockerMapper.ToDomain);
    }

    public async Task<string> GetLogsAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var parameters = new ContainerLogsParameters
        {
            ShowStdout = true,
            ShowStderr = true,
            Timestamps = true,
            Tail = "100"
        };

        var multiplexedStream = await _client.Containers.GetContainerLogsAsync(
            containerId,
            false,
            parameters,
            cancellationToken);

        var buffer = new byte[4096];
        var output = new StringBuilder();

        while (true)
        {
            var readResult = await multiplexedStream.ReadOutputAsync(buffer, 0, buffer.Length, cancellationToken);
            if (readResult.EOF)
                break;

            if (readResult.Count > 0)
            {
                var text = Encoding.UTF8.GetString(buffer, 0, readResult.Count);
                output.Append(text);
            }
        }

        return CleanDockerLogs(output.ToString());
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _client.Containers.StartContainerAsync(
            containerId,
            new ContainerStartParameters(),
            cancellationToken);
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _client.Containers.StopContainerAsync(
            containerId,
            new ContainerStopParameters { WaitBeforeKillSeconds = 10 },
            cancellationToken);
    }

    public async Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _client.Containers.RemoveContainerAsync(
            containerId,
            new ContainerRemoveParameters { Force = true },
            cancellationToken);
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

    private static string CleanDockerLogs(string logs)
    {
        var lines = logs.Split('\n');
        var cleanedLogs = new StringBuilder();

        foreach (var line in lines)
        {
            if (line.Length > 8)
            {
                cleanedLogs.AppendLine(line[8..]);
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                cleanedLogs.AppendLine(line);
            }
        }

        return cleanedLogs.ToString();
    }
}
