using FancyContainerConsole.Domain.Entities;

namespace FancyContainerConsole.Application.Interfaces;

public interface IVolumeRepository
{
    Task<IEnumerable<Volume>> GetVolumesAsync(CancellationToken cancellationToken = default);
    Task<Volume?> GetVolumeByNameAsync(string name, CancellationToken cancellationToken = default);
    Task DeleteVolumeAsync(string name, CancellationToken cancellationToken = default);
}
