using FancyContainerConsole.UI.Localization;
using Spectre.Console;

namespace FancyContainerConsole.UI.Helpers;

public class TableSelectionResult<T> where T : class
{
    public T? SelectedItem { get; set; }
    public string? Action { get; set; }
    public bool IsBack { get; set; }
}

public static class TableSelectionHelper
{
    public static TableSelectionResult<T> SelectFromTable<T>(
        IList<T> items,
        Action<Table, IList<T>, int, ILocalizationService> renderTable,
        string title,
        string backToMenuText,
        string helpText,
        Dictionary<ConsoleKey, string> actionKeys,
        ILocalizationService localization) where T : class
    {
        var selectedIndex = -1; // -1 means "Back to Main Menu"
        var maxIndex = items.Count - 1;

        while (true)
        {
            // Clear screen and display title
            DisplayHelper.DisplayTitle(title, localization);

            // Render "Back to Main Menu" option
            if (selectedIndex == -1)
            {
                AnsiConsole.MarkupLine($"[yellow]>[/] [black on yellow] {Markup.Escape(backToMenuText)} [/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"  {Markup.Escape(backToMenuText)}");
            }

            AnsiConsole.WriteLine();

            // Render table with selection
            var table = new Table();
            renderTable(table, items, selectedIndex, localization);
            AnsiConsole.Write(table);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[grey]{helpText}[/]");

            // Wait for key press
            var key = Console.ReadKey(true);

            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    selectedIndex = selectedIndex <= -1 ? maxIndex : selectedIndex - 1;
                    break;

                case ConsoleKey.DownArrow:
                    selectedIndex = selectedIndex >= maxIndex ? -1 : selectedIndex + 1;
                    break;

                case ConsoleKey.Enter:
                    if (selectedIndex == -1)
                    {
                        return new TableSelectionResult<T> { IsBack = true };
                    }
                    break;

                case ConsoleKey.Escape:
                    return new TableSelectionResult<T> { IsBack = true };

                default:
                    // Check if this is an action key and an item is selected
                    if (selectedIndex >= 0 && actionKeys.ContainsKey(key.Key))
                    {
                        return new TableSelectionResult<T>
                        {
                            SelectedItem = items[selectedIndex],
                            Action = actionKeys[key.Key]
                        };
                    }
                    break;
            }
        }
    }
}
