using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.UI.Helpers;
using Spectre.Console;

namespace FancyContainerConsole.UI.Menus;

public sealed class InteractiveContainerMenu
{
    private readonly IContainerService _containerService;

    public InteractiveContainerMenu(IContainerService containerService)
    {
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            DisplayHelper.DisplayTitle("Interactive Container Dashboard");

            var containers = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("[yellow]Loading containers...[/]", async ctx =>
                {
                    return (await _containerService.GetAllContainersAsync()).ToList();
                });

            if (!containers.Any())
            {
                DisplayHelper.DisplayError("No containers found.");
                AnsiConsole.MarkupLine("[grey]Press any key to return to main menu...[/]");
                Console.ReadKey(true);
                return;
            }

            DisplayHelper.DisplayContainers(containers);

            AnsiConsole.WriteLine();

            var allChoices = new List<object> { "← Back to Main Menu" };
            allChoices.AddRange(containers);

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .Title("[blue]Select a container (use arrow keys):[/]")
                    .AddChoices(allChoices)
                    .UseConverter(choice => choice is ContainerDto c
                        ? $"{Markup.Escape(c.Name)} ({Markup.Escape(c.State)})"
                        : choice.ToString()!)
            );

            if (selectedOption is string)
            {
                return;
            }

            var selectedContainer = (ContainerDto)selectedOption;
            var action = await PromptForActionAsync();

            if (action == ActionType.Back)
            {
                return;
            }

            await HandleActionAsync(selectedContainer, action);
        }
    }

    private Task<ActionType> PromptForActionAsync()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[yellow]Press L (logs), S (start/stop), D (delete), C (details), or ESC (back)[/]");

        while (true)
        {
            var key = Console.ReadKey(true);

            var action = key.Key switch
            {
                ConsoleKey.L => ActionType.ViewLogs,
                ConsoleKey.S => ActionType.StartStop,
                ConsoleKey.D => ActionType.Delete,
                ConsoleKey.C => ActionType.ViewDetails,
                ConsoleKey.Escape => ActionType.Back,
                _ => (ActionType?)null
            };

            if (action.HasValue)
            {
                return Task.FromResult(action.Value);
            }
        }
    }

    private async Task HandleActionAsync(ContainerDto container, ActionType action)
    {
        switch (action)
        {
            case ActionType.ViewLogs:
                await ViewLogsAsync(container.Id);
                break;
            case ActionType.StartStop:
                await StartStopContainerAsync(container);
                break;
            case ActionType.Delete:
                await DeleteContainerAsync(container);
                break;
            case ActionType.ViewDetails:
                await ViewDetailsAsync(container);
                break;
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

    private async Task StartStopContainerAsync(ContainerDto container)
    {
        var isRunning = container.State.ToLowerInvariant() == "running";
        var action = isRunning ? "stop" : "start";

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync($"[yellow]{char.ToUpper(action[0]) + action[1..]}ping container...[/]", async ctx =>
            {
                try
                {
                    if (isRunning)
                    {
                        await _containerService.StopContainerAsync(container.Id);
                        DisplayHelper.DisplaySuccess("Container stopped successfully");
                    }
                    else
                    {
                        await _containerService.StartContainerAsync(container.Id);
                        DisplayHelper.DisplaySuccess("Container started successfully");
                    }
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError($"Failed to {action} container: {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private async Task DeleteContainerAsync(ContainerDto container)
    {
        var confirm = AnsiConsole.Confirm(
            $"[red]Are you sure you want to delete container '{container.Name}'?[/]",
            false);

        if (!confirm)
        {
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[yellow]Deleting container...[/]", async ctx =>
            {
                try
                {
                    await _containerService.DeleteContainerAsync(container.Id);
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

    private async Task ViewDetailsAsync(ContainerDto container)
    {
        DisplayHelper.DisplayTitle($"Container Details: {container.Name}");
        DisplayHelper.DisplayContainerDetails(container);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);

        await Task.CompletedTask;
    }

    private enum ActionType
    {
        ViewLogs,
        StartStop,
        Delete,
        ViewDetails,
        Back
    }
}
