using Docker.DotNet;
using Docker.DotNet.Models;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Domain.Entities;
using FancyContainerConsole.Infrastructure.Mappers;
using System.Runtime.InteropServices;

namespace FancyContainerConsole.Infrastructure.Docker;

public sealed class DockerImageAdapter : IImageRepository, IDisposable
{
    private readonly DockerClient _client;

    public DockerImageAdapter()
    {
        var dockerUri = GetDockerUri();
        _client = new DockerClientConfiguration(dockerUri).CreateClient();
    }

    public async Task<IEnumerable<Image>> GetImagesAsync(CancellationToken cancellationToken = default)
    {
        // Get all images
        var parameters = new ImagesListParameters { All = false }; // Only show non-intermediate images
        var dockerImages = await _client.Images.ListImagesAsync(parameters, cancellationToken);

        // Get all containers to check which images are in use
        var containersListParameters = new ContainersListParameters { All = true };
        var containers = await _client.Containers.ListContainersAsync(containersListParameters, cancellationToken);

        // Build a set of image IDs that are in use
        var imagesInUse = new HashSet<string>();
        foreach (var container in containers)
        {
            if (!string.IsNullOrEmpty(container.ImageID))
            {
                imagesInUse.Add(container.ImageID);
            }
        }

        // Map Docker images to domain entities
        var images = new List<Image>();
        foreach (var dockerImage in dockerImages)
        {
            var isInUse = imagesInUse.Contains(dockerImage.ID);
            images.Add(DockerMapper.ToDomain(dockerImage, isInUse));
        }

        return images;
    }

    public async Task<Image?> GetImageByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var image = await _client.Images.InspectImageAsync(id, cancellationToken);

            // Check if image is in use by any container
            var containersListParameters = new ContainersListParameters { All = true };
            var containers = await _client.Containers.ListContainersAsync(containersListParameters, cancellationToken);

            var isInUse = containers.Any(c => c.ImageID == image.ID);

            return DockerMapper.ToDomain(image, isInUse);
        }
        catch
        {
            return null;
        }
    }

    public async Task DeleteImageAsync(string id, CancellationToken cancellationToken = default)
    {
        await _client.Images.DeleteImageAsync(id, new ImageDeleteParameters { Force = false }, cancellationToken);
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
