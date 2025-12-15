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

        // Get all volume sizes at once using docker system df
        var volumeSizes = await GetAllVolumeSizesAsync();

        // Inspect each volume to get full data and check if in use
        var volumes = new List<Volume>();
        foreach (var vol in response.Volumes)
        {
            try
            {
                var inspectedVolume = await _client.Volumes.InspectAsync(vol.Name, cancellationToken);
                var isInUse = volumesInUse.Contains(vol.Name);

                // Use size from docker system df if UsageData is not available
                long size = inspectedVolume.UsageData?.Size ?? 0L;
                if (size == 0 && volumeSizes.TryGetValue(vol.Name, out var calculatedSize))
                {
                    size = calculatedSize;
                }

                volumes.Add(DockerMapper.ToDomain(inspectedVolume, isInUse, size));
            }
            catch
            {
                // If inspection fails, use the basic volume data
                var isInUse = volumesInUse.Contains(vol.Name);
                long size = volumeSizes.TryGetValue(vol.Name, out var calculatedSize) ? calculatedSize : 0L;
                volumes.Add(DockerMapper.ToDomain(vol, isInUse, size));
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

            // Use size from docker system df if UsageData is not available
            long size = volume.UsageData?.Size ?? 0L;
            if (size == 0)
            {
                var volumeSizes = await GetAllVolumeSizesAsync();
                if (volumeSizes.TryGetValue(name, out var calculatedSize))
                {
                    size = calculatedSize;
                }
            }

            return DockerMapper.ToDomain(volume, isInUse, size);
        }
        catch
        {
            return null;
        }
    }

    private async Task<Dictionary<string, long>> GetAllVolumeSizesAsync()
    {
        var volumeSizes = new Dictionary<string, long>();

        try
        {
            // Use docker system df -v to get volume sizes
            // This doesn't require elevated privileges and is more efficient than calling it per volume
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "system df -v --format \"{{json .Volumes}}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(startInfo);
            if (process == null)
            {
                return volumeSizes;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                return volumeSizes;
            }

            // Parse JSON output to build a dictionary of volume names to sizes
            using var document = System.Text.Json.JsonDocument.Parse(output);
            var volumes = document.RootElement;

            if (volumes.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var vol in volumes.EnumerateArray())
                {
                    if (vol.TryGetProperty("Name", out var name) &&
                        vol.TryGetProperty("Size", out var size))
                    {
                        var volumeName = name.GetString();
                        var sizeStr = size.GetString() ?? "0B";

                        if (!string.IsNullOrEmpty(volumeName))
                        {
                            volumeSizes[volumeName] = ParseDockerSize(sizeStr);
                        }
                    }
                }
            }

            return volumeSizes;
        }
        catch
        {
            // If we can't get sizes, return empty dictionary
            return volumeSizes;
        }
    }

    private static long ParseDockerSize(string sizeStr)
    {
        try
        {
            // Parse Docker size format (e.g., "1.014GB", "526.1kB", "48.41MB", "0B")
            sizeStr = sizeStr.Trim().ToUpperInvariant();

            if (sizeStr == "0B" || string.IsNullOrEmpty(sizeStr))
            {
                return 0L;
            }

            // Extract number and unit
            var numStr = new string(sizeStr.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            var unit = new string(sizeStr.SkipWhile(c => char.IsDigit(c) || c == '.').ToArray());

            if (!double.TryParse(numStr, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number))
            {
                return 0L;
            }

            // Convert to bytes
            return unit switch
            {
                "B" => (long)number,
                "KB" => (long)(number * 1024),
                "MB" => (long)(number * 1024 * 1024),
                "GB" => (long)(number * 1024 * 1024 * 1024),
                "TB" => (long)(number * 1024L * 1024 * 1024 * 1024),
                _ => 0L
            };
        }
        catch
        {
            return 0L;
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
