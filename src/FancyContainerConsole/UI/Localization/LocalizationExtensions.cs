using System.Globalization;

namespace FancyContainerConsole.UI.Localization;

public static class LocalizationExtensions
{
    /// <summary>
    /// Parses culture string (e.g., "fr", "fr-FR", "en-US") to CultureInfo.
    /// </summary>
    public static CultureInfo? ParseCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
            return null;

        try
        {
            return CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }
    }

    /// <summary>
    /// Resolves culture from multiple sources in priority order.
    /// </summary>
    public static CultureInfo ResolveCulture(string? cliArgument, string? envVariable)
    {
        // 1. Try CLI argument
        var culture = ParseCulture(cliArgument);
        if (culture != null) return culture;

        // 2. Try environment variable
        culture = ParseCulture(envVariable);
        if (culture != null) return culture;

        // 3. Use system culture
        return CultureInfo.CurrentUICulture;
    }
}
