using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Domain.Entities;

namespace FancyContainerConsole.Application.Mappers;

public static class ImageMapper
{
    public static ImageDto ToDto(Image image)
    {
        ArgumentNullException.ThrowIfNull(image);

        return new ImageDto(
            image.Id.Value,
            image.Repository,
            image.Tag,
            image.Size,
            image.CreatedAt,
            image.InUse
        );
    }

    public static IEnumerable<ImageDto> ToDto(IEnumerable<Image> images)
    {
        return images.Select(ToDto);
    }
}
