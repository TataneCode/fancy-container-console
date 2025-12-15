using FancyContainerConsole.Domain.Entities;

namespace FancyContainerConsole.Application.Interfaces;

public interface IImageRepository
{
    Task<IEnumerable<Image>> GetImagesAsync(CancellationToken cancellationToken = default);
    Task<Image?> GetImageByIdAsync(string id, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string id, CancellationToken cancellationToken = default);
}
