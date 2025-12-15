using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.UI.Helpers;
using FancyContainerConsole.UI.Localization;
using Spectre.Console;

namespace FancyContainerConsole.UI.Menus;

public sealed class MainMenu
{
    private readonly IContainerService _containerService;
    private readonly IVolumeService _volumeService;
    private readonly IImageService _imageService;
    private readonly ILocalizationService _localization;

    public MainMenu(IContainerService containerService, IVolumeService volumeService, IImageService imageService, ILocalizationService localization)
    {
        _containerService = containerService ?? throw new ArgumentNullException(nameof(containerService));
        _volumeService = volumeService ?? throw new ArgumentNullException(nameof(volumeService));
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            DisplayHelper.DisplayTitle(_localization.Get("UI_Title_MainMenu"), _localization);

            var manageContainerText = _localization.Get("UI_Choice_ManageContainer");
            var manageVolumesText = _localization.Get("UI_Choice_ManageVolumes");
            var manageImagesText = _localization.Get("UI_Choice_ManageImages");
            var exitText = _localization.Get("UI_Choice_Exit");

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(_localization.Get("UI_Prompt_WhatToDo"))
                    .AddChoices(
                        manageContainerText,
                        manageVolumesText,
                        manageImagesText,
                        exitText
                    ));

            if (choice == manageContainerText)
            {
                await ManageContainerAsync();
            }
            else if (choice == manageVolumesText)
            {
                await ManageVolumesAsync();
            }
            else if (choice == manageImagesText)
            {
                await ManageImagesAsync();
            }
            else if (choice == exitText)
            {
                AnsiConsole.MarkupLine(_localization.Get("UI_Message_Goodbye"));
                return;
            }
        }
    }

    private async Task ManageVolumesAsync()
    {
        var volumeMenu = new VolumeMenu(_volumeService, _localization);
        await volumeMenu.ShowAsync();
    }

    private async Task ManageImagesAsync()
    {
        var imageMenu = new ImageMenu(_imageService, _localization);
        await imageMenu.ShowAsync();
    }

    private async Task ManageContainerAsync()
    {
        while (true)
        {
            DisplayHelper.DisplayTitle(_localization.Get("UI_Title_ManageContainer"), _localization);

            var containers = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_localization.Get("Container_Status_Loading"), async ctx =>
                {
                    return (await _containerService.GetAllContainersAsync()).ToList();
                });

            if (!containers.Any())
            {
                DisplayHelper.DisplayError(_localization.Get("Container_Error_NoContainersFound"), _localization);
                AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKeyReturn"));
                Console.ReadKey(true);
                return;
            }

            DisplayHelper.DisplayContainers(containers, _localization);

            AnsiConsole.WriteLine();

            var backText = _localization.Get("UI_Choice_BackToMainMenu");
            var allChoices = new List<object> { backText };
            allChoices.AddRange(containers);

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .Title(_localization.Get("UI_Prompt_SelectContainer"))
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
            var action = await PromptForContainerActionAsync();

            if (action == ContainerActionType.Back)
            {
                return;
            }

            await HandleContainerActionAsync(selectedContainer, action);
        }
    }

    private Task<ContainerActionType> PromptForContainerActionAsync()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Prompt_ContainerActions"));

        while (true)
        {
            var key = Console.ReadKey(true);

            var action = key.Key switch
            {
                ConsoleKey.L => ContainerActionType.ViewLogs,
                ConsoleKey.S => ContainerActionType.StartStop,
                ConsoleKey.D => ContainerActionType.Delete,
                ConsoleKey.C => ContainerActionType.ViewDetails,
                ConsoleKey.Escape => ContainerActionType.Back,
                _ => (ContainerActionType?)null
            };

            if (action.HasValue)
            {
                return Task.FromResult(action.Value);
            }
        }
    }

    private async Task HandleContainerActionAsync(ContainerDto container, ContainerActionType action)
    {
        switch (action)
        {
            case ContainerActionType.ViewLogs:
                await ViewLogsAsync(container.Id);
                break;
            case ContainerActionType.StartStop:
                await StartStopContainerAsync(container);
                break;
            case ContainerActionType.Delete:
                await DeleteContainerAsync(container);
                break;
            case ContainerActionType.ViewDetails:
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
                        DisplayHelper.DisplaySuccess(_localization.Get("Container_Success_Stopped"), _localization);
                    }
                    else
                    {
                        await _containerService.StartContainerAsync(container.Id);
                        DisplayHelper.DisplaySuccess(_localization.Get("Container_Success_Started"), _localization);
                    }
                }
                catch (Exception ex)
                {
                    var errorKey = isRunning ? "Container_Error_FailedToStop" : "Container_Error_FailedToStart";
                    DisplayHelper.DisplayError(_localization.Get(errorKey, ex.Message), _localization);
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
        Console.ReadKey(true);
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

    private enum ContainerActionType
    {
        ViewLogs,
        StartStop,
        Delete,
        ViewDetails,
        Back
    }
}
