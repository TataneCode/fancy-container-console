using FancyContainerConsole.Application.Interfaces;
using FancyContainerConsole.Application.Services;
using FancyContainerConsole.Infrastructure.Docker;
using FancyContainerConsole.UI.Menus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Spectre.Console;

var services = new ServiceCollection();

services.AddSingleton<IContainerRepository, DockerClientAdapter>();
services.AddSingleton<IContainerService, ContainerService>();
services.AddSingleton<IVolumeRepository, DockerVolumeAdapter>();
services.AddSingleton<IVolumeService, VolumeService>();
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
    AnsiConsole.WriteException(ex);
    AnsiConsole.MarkupLine("[red]An error occurred. Make sure Docker is running and accessible.[/]");

    if (OperatingSystem.IsLinux())
    {
        AnsiConsole.MarkupLine("[yellow]On Linux, you may need to run this application with sudo or add your user to the docker group.[/]");
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
