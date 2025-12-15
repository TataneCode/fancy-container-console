using FancyContainerConsole.Application.DTOs;
using Spectre.Console;

namespace FancyContainerConsole.UI.Helpers;

public static class DisplayHelper
{
    public static void DisplayTitle(string title)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText("Fancy Container")
            .Color(Color.Blue));

        AnsiConsole.Write(new Rule($"[blue]{title}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();
    }

    public static void DisplayContainers(IEnumerable<ContainerDto> containers)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Name[/]");
        table.AddColumn("[yellow]ID[/]");
        table.AddColumn("[yellow]Image[/]");
        table.AddColumn("[yellow]State[/]");
        table.AddColumn("[yellow]Networks[/]");
        table.AddColumn("[yellow]Ports[/]");
        table.AddColumn("[yellow]Size (MB)[/]");

        foreach (var container in containers)
        {
            var stateColor = GetStateColor(container.State);
            var shortId = container.Id.Length > 12 ? container.Id[..12] : container.Id;

            var networks = container.Networks.Any()
                ? string.Join(", ", container.Networks.Select(n => n.Name))
                : "N/A";

            var ports = container.PortMappings.Any()
                ? string.Join(", ", container.PortMappings.Select(p => $"{p.PublicPort}:{p.PrivatePort}"))
                : "N/A";

            var sizeMb = container.MemoryUsage / 1024 / 1024;

            table.AddRow(
                container.Name,
                shortId,
                container.Image,
                $"[{stateColor}]{container.State}[/]",
                networks,
                ports,
                sizeMb.ToString()
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayLogs(string logs)
    {
        // Escape markup to prevent Spectre.Console from parsing ANSI codes as markup
        var escapedLogs = Markup.Escape(logs);

        var panel = new Panel(escapedLogs)
        {
            Header = new PanelHeader("[yellow]Container Logs[/]"),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue)
        };

        AnsiConsole.Write(panel);
    }

    public static void DisplaySuccess(string message)
    {
        AnsiConsole.MarkupLine($"[green]✓ {message}[/]");
    }

    public static void DisplayError(string message)
    {
        AnsiConsole.MarkupLine($"[red]✗ {message}[/]");
    }

    public static void DisplayVolumes(IEnumerable<VolumeDto> volumes)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Name[/]");
        table.AddColumn("[yellow]Size (MB)[/]");
        table.AddColumn("[yellow]In Use[/]");
        table.AddColumn("[yellow]Created[/]");

        foreach (var volume in volumes)
        {
            var inUseColor = volume.InUse ? "green" : "grey";
            var inUseText = volume.InUse ? "Yes" : "No";
            var sizeMb = volume.Size / 1024 / 1024;

            table.AddRow(
                volume.Name,
                sizeMb.ToString(),
                $"[{inUseColor}]{inUseText}[/]",
                volume.CreatedAt != DateTime.MinValue
                    ? volume.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    : "N/A"
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayContainerDetails(ContainerDto container)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn("[yellow]Property[/]");
        table.AddColumn("[blue]Value[/]");

        table.AddRow("ID", container.Id);
        table.AddRow("Name", container.Name);
        table.AddRow("Image", container.Image);

        var stateColor = GetStateColor(container.State);
        table.AddRow("State", $"[{stateColor}]{container.State}[/]");

        var networks = container.Networks.Any()
            ? string.Join(", ", container.Networks.Select(n => $"{n.Name} ({n.IpAddress})"))
            : "N/A";
        table.AddRow("Networks", networks);

        var ports = container.PortMappings.Any()
            ? string.Join("\n", container.PortMappings.Select(p => $"{p.PublicPort}:{p.PrivatePort}/{p.Type}"))
            : "N/A";
        table.AddRow("Port Mappings", ports);

        var sizeMb = container.MemoryUsage / 1024 / 1024;
        table.AddRow("Disk Size", $"{sizeMb} MB");

        var volumes = container.Volumes.Any()
            ? string.Join("\n", container.Volumes)
            : "N/A";
        table.AddRow("Volumes", volumes);

        table.AddRow("Created", container.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

        AnsiConsole.Write(table);
    }

    private static string GetStateColor(string state)
    {
        return state.ToLowerInvariant() switch
        {
            "running" => "green",
            "paused" => "yellow",
            "stopped" => "grey",
            "exited" => "grey",
            "dead" => "red",
            "created" => "blue",
            _ => "white"
        };
    }
}
