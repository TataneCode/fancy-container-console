using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Application.Services;
using FancyContainerConsole.Infrastructure.Docker;
using FancyContainerConsole.UI.Menus;
using FancyContainerConsole.UI.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

// Parse command-line arguments for culture
string? cultureArg = null;
for (int i = 0; i < args.Length; i++)
{
    if ((args[i] == "--culture" || args[i] == "-c") && i + 1 < args.Length)
    {
        cultureArg = args[i + 1];
        break;
    }
}

// Resolve culture from CLI, environment variable, or system
var envCulture = Environment.GetEnvironmentVariable("FANCY_CONTAINER_CULTURE");
var culture = LocalizationExtensions.ResolveCulture(cultureArg, envCulture);

// Configure services
var services = new ServiceCollection();

// Register localization service and set culture
services.AddSingleton<ILocalizationService>(sp =>
{
    var locService = new LocalizationService();
    locService.CurrentCulture = culture;
    return locService;
});

services.AddSingleton<IContainerRepository, DockerClientAdapter>();
services.AddSingleton<IContainerService, ContainerService>();
services.AddSingleton<IVolumeRepository, DockerVolumeAdapter>();
services.AddSingleton<IVolumeService, VolumeService>();
services.AddSingleton<IImageRepository, DockerImageAdapter>();
services.AddSingleton<IImageService, ImageService>();
services.AddSingleton<MainMenu>();

services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Warning);
});

var serviceProvider = services.BuildServiceProvider();

try
{
    var mainMenu = serviceProvider.GetRequiredService<MainMenu>();
    await mainMenu.ShowAsync();
}
catch (Exception ex)
{
    var loc = serviceProvider.GetRequiredService<ILocalizationService>();
    AnsiConsole.WriteException(ex);
    AnsiConsole.MarkupLine(loc.Get("Messages_Error_General"));

    if (OperatingSystem.IsLinux())
    {
        AnsiConsole.MarkupLine(loc.Get("Messages_Error_LinuxPermissions"));
    }
}
finally
{
    if (serviceProvider is IDisposable disposable)
    {
        disposable.Dispose();
    }
}

public partial class Program { }
