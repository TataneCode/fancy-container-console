using FancyContainerConsole.Domain.Entities;

namespace FancyContainerConsole.Application.Interfaces;

public interface IContainerRepository
{
    Task<IEnumerable<Container>> GetContainersAsync(CancellationToken cancellationToken = default);
    Task<string> GetLogsAsync(string containerId, CancellationToken cancellationToken = default);
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken = default);
}
