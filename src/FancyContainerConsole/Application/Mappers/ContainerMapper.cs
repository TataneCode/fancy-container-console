using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Domain.Entities;

namespace FancyContainerConsole.Application.Mappers;

public static class ContainerMapper
{
    public static ContainerDto ToDto(Container container)
    {
        ArgumentNullException.ThrowIfNull(container);

        var portMappings = container.PortMappings
            .Select(pm => new PortMappingDto(pm.PrivatePort, pm.PublicPort, pm.Type))
            .ToList();

        var networks = container.Networks
            .Select(n => new NetworkInfoDto(n.Name, n.IpAddress))
            .ToList();

        return new ContainerDto(
            container.Id.Value,
            container.Name,
            container.Image,
            container.State.ToString(),
            container.CreatedAt,
            container.Ports,
            portMappings,
            networks,
            container.MemoryUsage,
            container.Volumes
        );
    }

    public static IEnumerable<ContainerDto> ToDto(IEnumerable<Container> containers)
    {
        return containers.Select(ToDto);
    }
}
