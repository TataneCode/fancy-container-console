using FancyContainerConsole.UI.Localization;
using System.Globalization;
using Xunit;

namespace FancyContainerConsole.Tests.Localization;

public class LocalizationServiceTests
{
    [Fact]
    public void Get_WithValidKey_English_ReturnsLocalizedString()
    {
        // Arrange
        var service = new LocalizationService();
        service.CurrentCulture = CultureInfo.GetCultureInfo("en");

        // Act
        var result = service.Get("UI_Title_MainMenu");

        // Assert
        Assert.Equal("Main Menu", result);
    }

    [Fact]
    public void Get_WithValidKey_French_ReturnsLocalizedString()
    {
        // Arrange
        var service = new LocalizationService();
        service.CurrentCulture = CultureInfo.GetCultureInfo("fr");

        // Act
        var result = service.Get("UI_Title_MainMenu");

        // Assert
        Assert.Equal("Menu Principal", result);
    }

    [Fact]
    public void Get_WithInvalidKey_ReturnsMissingKeyIndicator()
    {
        // Arrange
        var service = new LocalizationService();

        // Act
        var result = service.Get("NonExistent_Key_Test");

        // Assert
        Assert.Contains("[MISSING RESOURCE FILE:", result);
    }

    [Fact]
    public void Get_WithInvalidKeyInValidResourceFile_ReturnsMissingKeyIndicator()
    {
        // Arrange
        var service = new LocalizationService();

        // Act
        var result = service.Get("UI_NonExistent_Key");

        // Assert
        Assert.Contains("[MISSING:", result);
    }

    [Fact]
    public void Get_WithFormatArguments_ReturnsFormattedString()
    {
        // Arrange
        var service = new LocalizationService();
        service.CurrentCulture = CultureInfo.GetCultureInfo("en");

        // Act
        var result = service.Get("Container_Error_FailedToStart", "timeout error");

        // Assert
        Assert.Contains("timeout error", result);
        Assert.Contains("Failed to start container:", result);
    }

    [Fact]
    public void CurrentCulture_ChangeCulture_ReturnsFrenchStrings()
    {
        // Arrange
        var service = new LocalizationService();

        // Act - Start with English
        service.CurrentCulture = CultureInfo.GetCultureInfo("en");
        var englishResult = service.Get("UI_Choice_Exit");

        // Switch to French
        service.CurrentCulture = CultureInfo.GetCultureInfo("fr");
        var frenchResult = service.Get("UI_Choice_Exit");

        // Assert
        Assert.Equal("Exit", englishResult);
        Assert.Equal("Quitter", frenchResult);
    }

    [Fact]
    public void Get_ErrorMessages_French_ReturnsTranslatedStrings()
    {
        // Arrange
        var service = new LocalizationService();
        service.CurrentCulture = CultureInfo.GetCultureInfo("fr");

        // Act
        var generalError = service.Get("Messages_Error_General");
        var linuxPermissions = service.Get("Messages_Error_LinuxPermissions");

        // Assert
        Assert.Contains("Une erreur s'est produite", generalError);
        Assert.Contains("Sous Linux", linuxPermissions);
    }

    [Fact]
    public void Get_TableHeaders_French_ReturnsTranslatedStrings()
    {
        // Arrange
        var service = new LocalizationService();
        service.CurrentCulture = CultureInfo.GetCultureInfo("fr");

        // Act
        var nameHeader = service.Get("Table_Header_Name");
        var stateHeader = service.Get("Table_Header_State");
        var sizeHeader = service.Get("Table_Header_Size");

        // Assert
        Assert.Contains("Nom", nameHeader);
        Assert.Contains("État", stateHeader);
        Assert.Contains("Taille", sizeHeader);
    }

    [Fact]
    public void KeyExists_WithValidKey_ReturnsTrue()
    {
        // Arrange
        var service = new LocalizationService();

        // Act
        var exists = service.KeyExists("UI_Title_MainMenu");

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public void KeyExists_WithInvalidKey_ReturnsFalse()
    {
        // Arrange
        var service = new LocalizationService();

        // Act
        var exists = service.KeyExists("Invalid_Key");

        // Assert
        Assert.False(exists);
    }
}
