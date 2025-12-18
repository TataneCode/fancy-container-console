using FancyContainerConsole.Application.DTOs;
using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.UI.Helpers;
using FancyContainerConsole.UI.Localization;
using Spectre.Console;

namespace FancyContainerConsole.UI.Menus;

public sealed class ImageMenu
{
    private readonly IImageService _imageService;
    private readonly ILocalizationService _localization;

    public ImageMenu(IImageService imageService, ILocalizationService localization)
    {
        _imageService = imageService ?? throw new ArgumentNullException(nameof(imageService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public async Task ShowAsync()
    {
        while (true)
        {
            var images = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_localization.Get("Image_Status_Loading"), async ctx =>
                {
                    return (await _imageService.GetAllImagesAsync()).ToList();
                });

            if (!images.Any())
            {
                DisplayHelper.DisplayTitle(_localization.Get("Image_Title_Management"), _localization);
                DisplayHelper.DisplayError(_localization.Get("Image_Error_NoImagesFound"), _localization);
                AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKeyReturn"));
                Console.ReadKey(true);
                return;
            }

            var actionKeys = new Dictionary<ConsoleKey, string>
            {
                { ConsoleKey.D, "Delete" },
                { ConsoleKey.C, "Details" }
            };

            var result = TableSelectionHelper.SelectFromTable(
                images,
                DisplayHelper.RenderImagesTable,
                _localization.Get("Image_Title_Management"),
                _localization.Get("UI_Choice_BackToMainMenu"),
                _localization.Get("Image_Prompt_Actions"),
                actionKeys,
                _localization
            );

            if (result.IsBack)
            {
                return;
            }

            if (result.SelectedItem != null && result.Action != null)
            {
                switch (result.Action)
                {
                    case "Delete":
                        await DeleteImageAsync(result.SelectedItem);
                        break;
                    case "Details":
                        await ViewDetailsAsync(result.SelectedItem);
                        break;
                }
            }
        }
    }

    private async Task DeleteImageAsync(ImageDto image)
    {
        if (image.InUse)
        {
            DisplayHelper.DisplayError(_localization.Get("Image_Error_InUse"), _localization);
            AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
            Console.ReadKey(true);
            return;
        }

        var confirm = AnsiConsole.Confirm(
            _localization.Get("Image_Confirm_Delete", $"{image.Repository}:{image.Tag}"),
            false);

        if (!confirm)
        {
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .StartAsync(_localization.Get("Image_Status_Deleting"), async ctx =>
            {
                try
                {
                    await _imageService.DeleteImageAsync(image.Id);
                    DisplayHelper.DisplaySuccess(_localization.Get("Image_Success_Deleted"), _localization);
                }
                catch (Exception ex)
                {
                    DisplayHelper.DisplayError(_localization.Get("Image_Error_FailedToDelete", ex.Message), _localization);
                }
            });

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
        Console.ReadKey(true);
    }

    private async Task ViewDetailsAsync(ImageDto image)
    {
        DisplayHelper.DisplayTitle(_localization.Get("Image_Title_Details"), _localization);
        DisplayHelper.DisplayImageDetails(image, _localization);

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKey"));
        Console.ReadKey(true);

        await Task.CompletedTask;
    }
}
