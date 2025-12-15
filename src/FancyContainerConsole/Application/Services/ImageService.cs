using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Application.Mappers;

namespace FancyContainerConsole.Application.Services;

public sealed class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;

    public ImageService(IImageRepository imageRepository)
    {
        _imageRepository = imageRepository ?? throw new ArgumentNullException(nameof(imageRepository));
    }

    public async Task<IEnumerable<ImageDto>> GetAllImagesAsync(CancellationToken cancellationToken = default)
    {
        var images = await _imageRepository.GetImagesAsync(cancellationToken);
        return ImageMapper.ToDto(images);
    }

    public async Task<ImageDto?> GetImageByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var image = await _imageRepository.GetImageByIdAsync(id, cancellationToken);
        return image != null ? ImageMapper.ToDto(image) : null;
    }

    public async Task DeleteImageAsync(string id, CancellationToken cancellationToken = default)
    {
        await _imageRepository.DeleteImageAsync(id, cancellationToken);
    }
}
