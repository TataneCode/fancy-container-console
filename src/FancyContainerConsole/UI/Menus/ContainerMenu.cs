using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.UI.Helpers;
using FancyContainerConsole.UI.Localization;
using Spectre.Console;

namespace FancyContainerConsole.UI.Menus;

public sealed class ContainerMenu
{
    private readonly IContainerService _containerService;
    private readonly ILocalizationService _localization;

    public ContainerMenu(IContainerService containerService, ILocalizationService localization)
    {
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var containers = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_localization.Get("Container_Status_Loading"), async ctx =>
                {
                    return (await _containerService.GetAllContainersAsync()).ToList();
                });

            if (!containers.Any())
            {
                DisplayHelper.DisplayTitle(_localization.Get("UI_Title_ManageContainer"), _localization);
                DisplayHelper.DisplayError(_localization.Get("Container_Error_NoContainersFound"), _localization);
                AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKeyReturn"));
                Console.ReadKey(true);
                return;
            }

            var actionKeys = new Dictionary<ConsoleKey, string>
            {
                { ConsoleKey.L, "ViewLogs" },
                { ConsoleKey.S, "StartStop" },
                { ConsoleKey.D, "Delete" },
                { ConsoleKey.C, "ViewDetails" }
            };

            var result = TableSelectionHelper.SelectFromTable(
                containers,
                DisplayHelper.RenderContainersTable,
                _localization.Get("UI_Title_ManageContainer"),
                _localization.Get("UI_Choice_BackToMainMenu"),
                _localization.Get("UI_Prompt_ContainerActions"),
                actionKeys,
                _localization
            );

            if (result.IsBack)
            {
                return;
            }

            if (result.SelectedItem != null && result.Action != null)
            {
                await HandleActionAsync(result.SelectedItem, result.Action);
            }
        }
    }

    private async Task HandleActionAsync(ContainerDto container, string action)
    {
        switch (action)
        {
            case "ViewLogs":
                await ViewLogsAsync(container.Id);
                break;
            case "StartStop":
                await StartStopContainerAsync(container);
                break;
            case "Delete":
                await DeleteContainerAsync(container);
                break;
            case "ViewDetails":
                await ViewDetailsAsync(container);
                break;
        }
    }

    private async Task ViewLogsAsync(string containerId)
    {
        DisplayHelper.DisplayTitle(_localization.Get("Container_Title_Logs"), _localization);

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(_localization.Get("Container_Status_FetchingLogs"), async ctx =>
            {
                try
                {
                    var logs = await _containerService.GetContainerLogsAsync(containerId);
                    ctx.Status(_localization.Get("Container_Status_DisplayingLogs"));
                    DisplayHelper.DisplayLogs(logs, _localization);
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError(_localization.Get("Container_Error_FailedToFetchLogs", ex.Message), _localization);
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
        Console.ReadKey(true);
    }

    private async Task StartStopContainerAsync(ContainerDto container)
    {
        var isRunning = container.State.ToLowerInvariant() == "running";
        var statusKey = isRunning ? "Container_Status_StoppingContainer" : "Container_Status_StartingContainer";

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(_localization.Get(statusKey), async ctx =>
            {
                try
                {
                    if (isRunning)
                    {
                        await _containerService.StopContainerAsync(container.Id);
                        ctx.Status(_localization.Get("Container_Success_Stopped"));
                        await Task.Delay(1000); // Brief pause to show success message
                    }
                    else
                    {
                        await _containerService.StartContainerAsync(container.Id);
                        ctx.Status(_localization.Get("Container_Success_Started"));
                        await Task.Delay(1000); // Brief pause to show success message
                    }
                }
                catch (Exception ex)
                {
                    var errorKey = isRunning ? "Container_Error_FailedToStop" : "Container_Error_FailedToStart";
                    ctx.Status(_localization.Get(errorKey, ex.Message));
                    await Task.Delay(2000); // Longer pause for errors
                }
            });
    }

    private async Task DeleteContainerAsync(ContainerDto container)
    {
        var confirm = AnsiConsole.Confirm(
            _localization.Get("Container_Confirm_Delete", container.Name),
            false);

        if (!confirm)
        {
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(_localization.Get("Container_Status_Deleting"), async ctx =>
            {
                try
                {
                    await _containerService.DeleteContainerAsync(container.Id);
                    DisplayHelper.DisplaySuccess(_localization.Get("Container_Success_Deleted"), _localization);
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError(_localization.Get("Container_Error_FailedToDelete", ex.Message), _localization);
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
        Console.ReadKey(true);
    }

    private async Task ViewDetailsAsync(ContainerDto container)
    {
        DisplayHelper.DisplayTitle(_localization.Get("Container_Title_Details", container.Name), _localization);
        DisplayHelper.DisplayContainerDetails(container, _localization);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
        Console.ReadKey(true);

        await Task.CompletedTask;
    }
}
