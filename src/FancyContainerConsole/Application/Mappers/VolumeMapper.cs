using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Domain.Entities;

namespace FancyContainerConsole.Application.Mappers;

public static class VolumeMapper
{
    public static VolumeDto ToDto(Volume volume)
    {
        ArgumentNullException.ThrowIfNull(volume);

        return new VolumeDto(
            volume.Id.Value,
            volume.Name,
            volume.Size,
            volume.InUse,
            volume.CreatedAt
        );
    }

    public static IEnumerable<VolumeDto> ToDto(IEnumerable<Volume> volumes)
    {
        return volumes.Select(ToDto);
    }
}
