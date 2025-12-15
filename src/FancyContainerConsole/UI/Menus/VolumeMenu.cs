using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.UI.Helpers;
using FancyContainerConsole.UI.Localization;
using Spectre.Console;

namespace FancyContainerConsole.UI.Menus;

public sealed class VolumeMenu
{
    private readonly IVolumeService _volumeService;
    private readonly ILocalizationService _localization;

    public VolumeMenu(IVolumeService volumeService, ILocalizationService localization)
    {
        _volumeService = volumeService ?? throw new ArgumentNullException(nameof(volumeService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            DisplayHelper.DisplayTitle(_localization.Get("Volume_Title_Management"), _localization);

            var volumes = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_localization.Get("Volume_Status_Loading"), async ctx =>
                {
                    return (await _volumeService.GetAllVolumesAsync()).ToList();
                });

            if (!volumes.Any())
            {
                DisplayHelper.DisplayError(_localization.Get("Volume_Error_NoVolumesFound"), _localization);
                AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKeyReturn"));
                Console.ReadKey(true);
                return;
            }

            DisplayHelper.DisplayVolumes(volumes, _localization);

            AnsiConsole.WriteLine();

            var backText = _localization.Get("UI_Choice_BackToMainMenu");
            var allChoices = new List<object> { backText };
            allChoices.AddRange(volumes);

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .Title(_localization.Get("Volume_Prompt_SelectVolume"))
                    .AddChoices(allChoices)
                    .UseConverter(choice => choice is VolumeDto v
                        ? $"{Markup.Escape(v.Name)} ({(v.InUse ? _localization.Get("Volume_Status_InUse") : _localization.Get("Volume_Status_NotInUse"))})"
                        : choice.ToString()!)
            );

            if (selectedOption is string)
            {
                return;
            }

            var selectedVolume = (VolumeDto)selectedOption;
            var action = await PromptForActionAsync();

            if (action == ActionType.Back)
            {
                return;
            }

            if (action == ActionType.Delete)
            {
                await DeleteVolumeAsync(selectedVolume);
            }
        }
    }

    private async Task<ActionType> PromptForActionAsync()
    {
        AnsiConsole.MarkupLine(_localization.Get("Volume_Prompt_Actions"));

        while (true)
        {
            var key = Console.ReadKey(true);

            return key.Key switch
            {
                ConsoleKey.D => ActionType.Delete,
                ConsoleKey.Escape => ActionType.Back,
                _ => await PromptForActionAsync()
            };
        }
    }

    private async Task DeleteVolumeAsync(VolumeDto volume)
    {
        if (volume.InUse)
        {
            DisplayHelper.DisplayError(_localization.Get("Volume_Error_InUse"), _localization);
            AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
            Console.ReadKey(true);
            return;
        }

        var confirm = AnsiConsole.Confirm(
            _localization.Get("Volume_Confirm_Delete", volume.Name),
            false);

        if (!confirm)
        {
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(_localization.Get("Volume_Status_Deleting"), async ctx =>
            {
                try
                {
                    await _volumeService.DeleteVolumeAsync(volume.Name);
                    DisplayHelper.DisplaySuccess(_localization.Get("Volume_Success_Deleted"), _localization);
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError(_localization.Get("Volume_Error_FailedToDelete", ex.Message), _localization);
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
        Console.ReadKey(true);
    }

    private enum ActionType
    {
        Delete,
        Back
    }
}
