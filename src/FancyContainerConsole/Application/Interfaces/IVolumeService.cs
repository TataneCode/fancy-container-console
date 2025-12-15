using FancyContainerConsole.Application.DTOs;

namespace FancyContainerConsole.Application.Interfaces;

public interface IVolumeService
{
    Task<IEnumerable<VolumeDto>> GetAllVolumesAsync(CancellationToken cancellationToken = default);
    Task<VolumeDto?> GetVolumeByNameAsync(string name, CancellationToken cancellationToken = default);
    Task DeleteVolumeAsync(string name, CancellationToken cancellationToken = default);
}
