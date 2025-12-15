using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.UI.Helpers;
using Spectre.Console;

namespace FancyContainerConsole.UI.Menus;

public sealed class MainMenu
{
    private readonly IContainerService _containerService;
    private readonly IVolumeService _volumeService;

    public MainMenu(IContainerService containerService, IVolumeService volumeService)
    {
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        _volumeService = volumeService ?? throw new ArgumentNullException(nameof(volumeService));
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            DisplayHelper.DisplayTitle("Main Menu");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[blue]What would you like to do?[/]")
                    .AddChoices(
                        "Interactive Container Dashboard",
                        "Manage container",
                        "Manage volumes",
                        "Exit"
                    ));

            switch (choice)
            {
                case "Interactive Container Dashboard":
                    await ShowInteractiveContainerDashboardAsync();
                    break;
                case "Manage container":
                    await ManageContainerAsync();
                    break;
                case "Manage volumes":
                    await ManageVolumesAsync();
                    break;
                case "Exit":
                    AnsiConsole.MarkupLine("[blue]Goodbye![/]");
                    return;
            }
        }
    }

    private async Task ShowInteractiveContainerDashboardAsync()
    {
        var interactiveMenu = new InteractiveContainerMenu(_containerService);
        await interactiveMenu.ShowAsync();
    }

    private async Task ManageVolumesAsync()
    {
        var volumeMenu = new VolumeMenu(_volumeService);
        await volumeMenu.ShowAsync();
    }

    private async Task ManageContainerAsync()
    {
        DisplayHelper.DisplayTitle("Manage Container");

        var containers = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[yellow]Loading containers...[/]", async ctx =>
            {
                return (await _containerService.GetAllContainersAsync()).ToList();
            });

        if (!containers.Any())
        {
            DisplayHelper.DisplayError("No containers found.");
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        var selectedContainer = AnsiConsole.Prompt(
            new SelectionPrompt<ContainerDto>()
                .Title("[blue]Select a container:[/]")
                .AddChoices(containers)
                .UseConverter(c => $"{c.Name} ({c.State})")
        );

        await ShowContainerActionsAsync(selectedContainer);
    }

    private async Task ShowContainerActionsAsync(ContainerDto container)
    {
        while (true)
        {
            DisplayHelper.DisplayTitle($"Container: {container.Name}");

            AnsiConsole.MarkupLine($"[blue]ID:[/] {container.Id}");
            AnsiConsole.MarkupLine($"[blue]Image:[/] {container.Image}");
            AnsiConsole.MarkupLine($"[blue]State:[/] {container.State}");
            AnsiConsole.WriteLine();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[blue]What would you like to do?[/]")
                    .AddChoices(
                        "View logs",
                        "Start container",
                        "Stop container",
                        "Delete container",
                        "Back to main menu"
                    ));

            switch (action)
            {
                case "View logs":
                    await ViewLogsAsync(container.Id);
                    break;
                case "Start container":
                    await StartContainerAsync(container.Id);
                    break;
                case "Stop container":
                    await StopContainerAsync(container.Id);
                    break;
                case "Delete container":
                    if (await ConfirmDeleteAsync(container))
                    {
                        await DeleteContainerAsync(container.Id);
                        return;
                    }
                    break;
                case "Back to main menu":
                    return;
            }
        }
    }

    private async Task ViewLogsAsync(string containerId)
    {
        DisplayHelper.DisplayTitle("Container Logs");

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[yellow]Fetching logs...[/]", async ctx =>
            {
                try
                {
                    var logs = await _containerService.GetContainerLogsAsync(containerId);
                    ctx.Status("[green]Displaying logs[/]");
                    DisplayHelper.DisplayLogs(logs);
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError($"Failed to fetch logs: {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private async Task StartContainerAsync(string containerId)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[yellow]Starting container...[/]", async ctx =>
            {
                try
                {
                    await _containerService.StartContainerAsync(containerId);
                    DisplayHelper.DisplaySuccess("Container started successfully");
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError($"Failed to start container: {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private async Task StopContainerAsync(string containerId)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[yellow]Stopping container...[/]", async ctx =>
            {
                try
                {
                    await _containerService.StopContainerAsync(containerId);
                    DisplayHelper.DisplaySuccess("Container stopped successfully");
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError($"Failed to stop container: {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private async Task DeleteContainerAsync(string containerId)
    {
        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[yellow]Deleting container...[/]", async ctx =>
            {
                try
                {
                    await _containerService.DeleteContainerAsync(containerId);
                    DisplayHelper.DisplaySuccess("Container deleted successfully");
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError($"Failed to delete container: {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private static Task<bool> ConfirmDeleteAsync(ContainerDto container)
    {
        var confirm = AnsiConsole.Confirm(
            $"[red]Are you sure you want to delete container '{container.Name}'?[/]",
            false);

        return Task.FromResult(confirm);
    }
}
