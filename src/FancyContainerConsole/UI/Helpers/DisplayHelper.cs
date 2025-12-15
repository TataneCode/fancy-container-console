using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.UI.Localization;
using Spectre.Console;

namespace FancyContainerConsole.UI.Helpers;

public static class DisplayHelper
{
    public static void DisplayTitle(string title, ILocalizationService localization)
    {
        AnsiConsole.Clear();
        AnsiConsole.Write(new FigletText(localization.Get("Messages_Title_FancyContainer"))
            .Color(Color.Blue));

        AnsiConsole.Write(new Rule($"[blue]{title}[/]").RuleStyle("grey"));
        AnsiConsole.WriteLine();
    }

    public static void DisplayContainers(IEnumerable<ContainerDto> containers, ILocalizationService localization)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(localization.Get("Table_Header_Name"));
        table.AddColumn(localization.Get("Table_Header_ID"));
        table.AddColumn(localization.Get("Table_Header_Image"));
        table.AddColumn(localization.Get("Table_Header_State"));
        table.AddColumn(localization.Get("Table_Header_Networks"));
        table.AddColumn(localization.Get("Table_Header_Ports"));
        table.AddColumn(localization.Get("Table_Header_Size"));

        foreach (var container in containers)
        {
            var stateColor = GetStateColor(container.State);
            var shortId = container.Id.Length > 12 ? container.Id[..12] : container.Id;

            var networks = container.Networks.Any()
                ? string.Join(", ", container.Networks.Select(n => n.Name))
                : localization.Get("Table_Value_NotAvailable");

            var ports = container.PortMappings.Any()
                ? string.Join(", ", container.PortMappings.Select(p => $"{p.PublicPort}:{p.PrivatePort}"))
                : localization.Get("Table_Value_NotAvailable");

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

    public static void DisplayLogs(string logs, ILocalizationService localization)
    {
        // Escape markup to prevent Spectre.Console from parsing ANSI codes as markup
        var escapedLogs = Markup.Escape(logs);

        var panel = new Panel(escapedLogs)
        {
            Header = new PanelHeader(localization.Get("Table_Panel_ContainerLogs")),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Blue)
        };

        AnsiConsole.Write(panel);
    }

    public static void DisplaySuccess(string message, ILocalizationService localization)
    {
        AnsiConsole.MarkupLine(localization.Get("Messages_Success_Prefix", message));
    }

    public static void DisplayError(string message, ILocalizationService localization)
    {
        AnsiConsole.MarkupLine(localization.Get("Messages_Error_Prefix", message));
    }

    public static void DisplayVolumes(IEnumerable<VolumeDto> volumes, ILocalizationService localization)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(localization.Get("Table_Header_Name"));
        table.AddColumn(localization.Get("Table_Header_Size"));
        table.AddColumn(localization.Get("Table_Header_InUse"));
        table.AddColumn(localization.Get("Table_Header_Created"));

        foreach (var volume in volumes)
        {
            var inUseColor = volume.InUse ? "green" : "grey";
            var inUseText = volume.InUse
                ? localization.Get("Table_Value_Yes")
                : localization.Get("Table_Value_No");
            var sizeMb = volume.Size / 1024 / 1024;

            table.AddRow(
                volume.Name,
                sizeMb.ToString(),
                $"[{inUseColor}]{inUseText}[/]",
                volume.CreatedAt != DateTime.MinValue
                    ? volume.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                    : localization.Get("Table_Value_NotAvailable")
            );
        }

        AnsiConsole.Write(table);
    }

    public static void DisplayContainerDetails(ContainerDto container, ILocalizationService localization)
    {
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.AddColumn(localization.Get("Table_Header_Property"));
        table.AddColumn(localization.Get("Table_Header_Value"));

        table.AddRow(localization.Get("Table_Property_ID"), container.Id);
        table.AddRow(localization.Get("Table_Property_Name"), container.Name);
        table.AddRow(localization.Get("Table_Property_Image"), container.Image);

        var stateColor = GetStateColor(container.State);
        table.AddRow(localization.Get("Table_Property_State"), $"[{stateColor}]{container.State}[/]");

        var networks = container.Networks.Any()
            ? string.Join(", ", container.Networks.Select(n => $"{n.Name} ({n.IpAddress})"))
            : localization.Get("Table_Value_NotAvailable");
        table.AddRow(localization.Get("Table_Property_Networks"), networks);

        var ports = container.PortMappings.Any()
            ? string.Join("\n", container.PortMappings.Select(p => $"{p.PublicPort}:{p.PrivatePort}/{p.Type}"))
            : localization.Get("Table_Value_NotAvailable");
        table.AddRow(localization.Get("Table_Property_PortMappings"), ports);

        var sizeMb = container.MemoryUsage / 1024 / 1024;
        table.AddRow(localization.Get("Table_Property_DiskSize"), $"{sizeMb} {localization.Get("Table_Unit_MB")}");

        var volumes = container.Volumes.Any()
            ? string.Join("\n", container.Volumes)
            : localization.Get("Table_Value_NotAvailable");
        table.AddRow(localization.Get("Table_Property_Volumes"), volumes);

        table.AddRow(localization.Get("Table_Property_Created"), container.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));

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
