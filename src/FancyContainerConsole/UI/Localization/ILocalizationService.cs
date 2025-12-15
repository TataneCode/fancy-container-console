using System.Globalization;

namespace FancyContainerConsole.UI.Localization;

public interface ILocalizationService
{
    /// <summary>
    /// Gets the localized string for the specified key.
    /// </summary>
    string Get(string key);

    /// <summary>
    /// Gets the localized string for the specified key with format arguments.
    /// </summary>
    string Get(string key, params object[] args);

    /// <summary>
    /// Gets or sets the current culture.
    /// </summary>
    CultureInfo CurrentCulture { get; set; }

    /// <summary>
    /// Checks if a key exists in the resource files.
    /// </summary>
    bool KeyExists(string key);
}
