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

    private async Task ManageContainerAsync()
    {
        var containerMenu = new ContainerMenu(_containerService, _localization);
        await containerMenu.ShowAsync();
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
}
