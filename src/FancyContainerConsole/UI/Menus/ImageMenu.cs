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
            DisplayHelper.DisplayTitle(_localization.Get("Image_Title_Management"), _localization);

            var images = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .StartAsync(_localization.Get("Image_Status_Loading"), async ctx =>
                {
                    return (await _imageService.GetAllImagesAsync()).ToList();
                });

            if (!images.Any())
            {
                DisplayHelper.DisplayError(_localization.Get("Image_Error_NoImagesFound"), _localization);
                AnsiConsole.MarkupLine(_localization.Get("UI_Message_PressAnyKeyReturn"));
                Console.ReadKey(true);
                return;
            }

            DisplayHelper.DisplayImages(images, _localization);

            AnsiConsole.WriteLine();

            var backText = _localization.Get("UI_Choice_BackToMainMenu");
            var allChoices = new List<object> { backText };
            allChoices.AddRange(images);

            var selectedOption = AnsiConsole.Prompt(
                new SelectionPrompt<object>()
                    .Title(_localization.Get("Image_Prompt_SelectImage"))
                    .AddChoices(allChoices)
                    .UseConverter(choice => choice is ImageDto img
                        ? $"{Markup.Escape(img.Repository)}:{Markup.Escape(img.Tag)} ({(img.InUse ? _localization.Get("Image_Status_InUse") : _localization.Get("Image_Status_NotInUse"))})"
                        : choice.ToString()!)
            );

            if (selectedOption is string)
            {
                return;
            }

            var selectedImage = (ImageDto)selectedOption;
            var action = await PromptForActionAsync();

            if (action == ActionType.Back)
            {
                return;
            }

            if (action == ActionType.Delete)
            {
                await DeleteImageAsync(selectedImage);
            }
            else if (action == ActionType.Details)
            {
                await ViewDetailsAsync(selectedImage);
            }
        }
    }

    private async Task<ActionType> PromptForActionAsync()
    {
        AnsiConsole.MarkupLine(_localization.Get("Image_Prompt_Actions"));

        while (true)
        {
            var key = Console.ReadKey(true);

            return key.Key switch
            {
                ConsoleKey.D => ActionType.Delete,
                ConsoleKey.C => ActionType.Details,
                ConsoleKey.Escape => ActionType.Back,
                _ => await PromptForActionAsync()
            };
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

    private enum ActionType
    {
        Delete,
        Details,
        Back
    }
}
