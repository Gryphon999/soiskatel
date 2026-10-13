using System.Globalization;

namespace Soiskatel.Mcp.Tracker;

/// <summary>Даты в файлах трекера: ГГГГ-ММ-ДД, независимо от языка системы.</summary>
public static class IsoDate
{
    private const string Pattern = "yyyy-MM-dd";

    public static string Format(DateOnly date) => date.ToString(Pattern, CultureInfo.InvariantCulture);

    public static bool TryParse(string text, out DateOnly date) =>
        DateOnly.TryParseExact(text.Trim(), Pattern, CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}
