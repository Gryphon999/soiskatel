namespace Soiskatel.Mcp.Tracker;

/// <summary>Сравнение названий компаний: «ООО «Ромашка»» и «Ромашка» одна и та же компания.</summary>
public static class CompanyName
{
    /// <summary>Правовые формы, которые не влияют на сравнение.</summary>
    public static IReadOnlySet<string> LegalForms { get; } =
        new HashSet<string> { "ооо", "оао", "зао", "пао", "ао", "ип", "llc", "inc", "ltd", "gmbh", "corp" };

    /// <summary>
    /// Нижний регистр, «ё» как «е», вместо любого знака, кроме букв и цифр, пробел;
    /// слова из LegalForms выбрасываются; оставшиеся слова через один пробел.
    /// </summary>
    public static string Normalize(string name)
    {
        var lower = name.ToLowerInvariant().Replace('ё', 'е');
        var spaced = new string(lower.Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ').ToArray());
        var words = spaced.Split(' ', StringSplitOptions.RemoveEmptyEntries).Where(w => !LegalForms.Contains(w));
        return string.Join(' ', words);
    }

    /// <summary>Одна и та же компания: нормализованные названия совпадают и не пустые.</summary>
    public static bool AreSame(string a, string b)
    {
        var x = Normalize(a);
        return x.Length > 0 && x == Normalize(b);
    }

    /// <summary>Возможно, одна компания: одно название начинается с другого как с отдельного слова («яндекс маркет» и «яндекс»).</summary>
    public static bool IsPossibleMatch(string a, string b)
    {
        var x = Normalize(a);
        var y = Normalize(b);
        if (x.Length == 0 || y.Length == 0 || x == y)
            return false;

        return x.StartsWith(y + " ", StringComparison.Ordinal) || y.StartsWith(x + " ", StringComparison.Ordinal);
    }
}
