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
        table.AddColumn("[yellow]Created[/]");

        foreach (var container in containers)
        {
            var stateColor = GetStateColor(container.State);
            var shortId = container.Id.Length > 12 ? container.Id[..12] : container.Id;

            table.AddRow(
                container.Name,
                shortId,
                container.Image,
                $"[{stateColor}]{container.State}[/]",
                container.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayLogs(string logs)
    {
        var panel = new Panel(logs)
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
