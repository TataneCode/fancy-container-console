using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Application.Mappers;

namespace FancyContainerConsole.Application.Services;

public sealed class VolumeService : IVolumeService
{
    private readonly IVolumeRepository _volumeRepository;

    public VolumeService(IVolumeRepository volumeRepository)
    {
        _volumeRepository = volumeRepository ?? throw new ArgumentNullException(nameof(volumeRepository));
    }

    public async Task<IEnumerable<VolumeDto>> GetAllVolumesAsync(CancellationToken cancellationToken = default)
    {
        var volumes = await _volumeRepository.GetVolumesAsync(cancellationToken);
        return VolumeMapper.ToDto(volumes);
    }

    public async Task<VolumeDto?> GetVolumeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var volume = await _volumeRepository.GetVolumeByNameAsync(name, cancellationToken);
        return volume != null ? VolumeMapper.ToDto(volume) : null;
    }

    public async Task DeleteVolumeAsync(string name, CancellationToken cancellationToken = default)
    {
        await _volumeRepository.DeleteVolumeAsync(name, cancellationToken);
    }
}
