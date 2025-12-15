using FancyContainerConsole.Application.DTOs;

namespace FancyContainerConsole.Application.Interfaces;

public interface IImageService
{
    Task<IEnumerable<ImageDto>> GetAllImagesAsync(CancellationToken cancellationToken = default);
    Task<ImageDto?> GetImageByIdAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string id, CancellationToken cancellationToken = default);
}
