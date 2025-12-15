using System.Globalization;
using System.Resources;
using System.Reflection;

namespace FancyContainerConsole.UI.Localization;

public sealed class LocalizationService : ILocalizationService
{
    private CultureInfo _currentCulture;
    private readonly Dictionary<string, ResourceManager> _resourceManagers;

    public LocalizationService()
    {
        _currentCulture = CultureInfo.CurrentUICulture;

        // Initialize resource managers for each resource file
        var assembly = Assembly.GetExecutingAssembly();
        _resourceManagers = new Dictionary<string, ResourceManager>
        {
            ["UI"] = new ResourceManager("FancyContainerConsole.Resources.UI.Strings", assembly),
            ["Container"] = new ResourceManager("FancyContainerConsole.Resources.Container.Strings", assembly),
            ["Volume"] = new ResourceManager("FancyContainerConsole.Resources.Volume.Strings", assembly),
            ["Messages"] = new ResourceManager("FancyContainerConsole.Resources.Messages.Strings", assembly),
            ["Table"] = new ResourceManager("FancyContainerConsole.Resources.Table.Strings", assembly)
        };
    }

    public CultureInfo CurrentCulture
    {
        get => _currentCulture;
        set
        {
            _currentCulture = value ?? throw new ArgumentNullException(nameof(value));
            CultureInfo.CurrentUICulture = _currentCulture;
            CultureInfo.CurrentCulture = _currentCulture;
        }
    }

    public string Get(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or empty.", nameof(key));

        // Parse key format: {ResourceFile}_{Key}
        var parts = key.Split('_', 2);
        if (parts.Length < 2)
            return $"[MISSING: {key}]";

        var resourceFile = parts[0];
        var resourceKey = key; // Use full key in resource file

        if (!_resourceManagers.TryGetValue(resourceFile, out var manager))
            return $"[MISSING RESOURCE FILE: {resourceFile}]";

        var value = manager.GetString(resourceKey, _currentCulture);
        return value ?? $"[MISSING: {key}]";
    }

    public string Get(string key, params object[] args)
    {
        var template = Get(key);

        try
        {
            return string.Format(template, args);
        }
        catch (FormatException)
        {
            return $"[FORMAT ERROR: {key}]";
        }
    }

    public bool KeyExists(string key)
    {
        var value = Get(key);
        return !value.StartsWith("[MISSING");
    }
}
