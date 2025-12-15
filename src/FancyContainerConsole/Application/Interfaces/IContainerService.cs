using FancyContainerConsole.Application.DTOs;

namespace FancyContainerConsole.Application.Interfaces;

public interface IContainerService
{
    Task<IEnumerable<ContainerDto>> GetAllContainersAsync(CancellationToken cancellationToken = default);
    Task<string> GetContainerLogsAsync(string containerId, CancellationToken cancellationToken = default);
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken = default);
}
