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
            var volumes = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_localization.Get("Volume_Status_Loading"), async ctx =>
                {
                    return (await _volumeService.GetAllVolumesAsync()).ToList();
                });

            if (!volumes.Any())
            {
                DisplayHelper.DisplayTitle(_localization.Get("Volume_Title_Management"), _localization);
                DisplayHelper.DisplayError(_localization.Get("Volume_Error_NoVolumesFound"), _localization);
                AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKeyReturn"));
                Console.ReadKey(true);
                return;
            }

            var actionKeys = new Dictionary<ConsoleKey, string>
            {
                { ConsoleKey.D, "Delete" }
            };

            var result = TableSelectionHelper.SelectFromTable(
                volumes,
                DisplayHelper.RenderVolumesTable,
                _localization.Get("Volume_Title_Management"),
                _localization.Get("UI_Choice_BackToMainMenu"),
                _localization.Get("Volume_Prompt_Actions"),
                actionKeys,
                _localization
            );

            if (result.IsBack)
            {
                return;
            }

            if (result.SelectedItem != null && result.Action == "Delete")
            {
                await DeleteVolumeAsync(result.SelectedItem);
            }
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
}
