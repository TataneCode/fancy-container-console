using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.UI.Helpers;
using Spectre.Console;

namespace FancyContainerConsole.UI.Menus;

public sealed class VolumeMenu
{
    private readonly IVolumeService _volumeService;

    public VolumeMenu(IVolumeService volumeService)
    {
        _volumeService = volumeService ?? throw new ArgumentNullException(nameof(volumeService));
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            DisplayHelper.DisplayTitle("Volume Management");

            var volumes = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync("[yellow]Loading volumes...[/]", async ctx =>
                {
                    return (await _volumeService.GetAllVolumesAsync()).ToList();
                });

            if (!volumes.Any())
            {
                DisplayHelper.DisplayError("No volumes found.");
                AnsiConsole.MarkupLine("[grey]Press any key to return to main menu...[/]");
                Console.ReadKey(true);
                return;
            }

            DisplayHelper.DisplayVolumes(volumes);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[yellow]Select a volume or press ESC to return to main menu[/]");

            var allChoices = new List<object>(volumes);
            allChoices.Add("[Back to Main Menu]");

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .Title("[blue]Select a volume:[/]")
                    .AddChoices(allChoices)
                    .UseConverter(choice => choice is VolumeDto v ? $"{v.Name} ({(v.InUse ? "In use" : "Not in use")})" : choice.ToString()!)
            );

            if (selectedOption is string && selectedOption.ToString() == "[Back to Main Menu]")
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
        AnsiConsole.MarkupLine("[yellow]Press D (delete) or ESC (back)[/]");

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
            DisplayHelper.DisplayError("Cannot delete volume that is currently in use.");
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(true);
            return;
        }

        var confirm = AnsiConsole.Confirm(
            $"[red]Are you sure you want to delete volume '{volume.Name}'?[/]",
            false);

        if (!confirm)
        {
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync("[yellow]Deleting volume...[/]", async ctx =>
            {
                try
                {
                    await _volumeService.DeleteVolumeAsync(volume.Name);
                    DisplayHelper.DisplaySuccess("Volume deleted successfully");
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError($"Failed to delete volume: {ex.Message}");
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    private enum ActionType
    {
        Delete,
        Back
    }
}
