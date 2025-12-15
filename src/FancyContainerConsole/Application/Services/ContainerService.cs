using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Application.Mappers;

namespace FancyContainerConsole.Application.Services;

public sealed class ContainerService : IContainerService
{
    private readonly IContainerRepository _containerRepository;

    public ContainerService(IContainerRepository containerRepository)
    {
        _containerRepository = containerRepository ?? throw new ArgumentNullException(nameof(containerRepository));
    }

    public async Task<IEnumerable<ContainerDto>> GetAllContainersAsync(CancellationToken cancellationToken = default)
    {
        var containers = await _containerRepository.GetContainersAsync(cancellationToken);
        return ContainerMapper.ToDto(containers);
    }

    public async Task<string> GetContainerLogsAsync(string containerId, CancellationToken cancellationToken = default)
    {
        return await _containerRepository.GetLogsAsync(containerId, cancellationToken);
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _containerRepository.StartContainerAsync(containerId, cancellationToken);
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _containerRepository.StopContainerAsync(containerId, cancellationToken);
    }

    public async Task DeleteContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _containerRepository.DeleteContainerAsync(containerId, cancellationToken);
    }
}
