using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Domain.Entities;

namespace FancyContainerConsole.Application.Mappers;

public static class ContainerMapper
{
    public static ContainerDto ToDto(Container container)
    {
        ArgumentNullException.ThrowIfNull(container);

        return new ContainerDto(
            container.Id.Value,
            container.Name,
            container.Image,
            container.State.ToString(),
            container.CreatedAt,
            container.Ports
        );
    }

    public static IEnumerable<ContainerDto> ToDto(IEnumerable<Container> containers)
    {
        return containers.Select(ToDto);
    }
}
