using System.Text.RegularExpressions;

namespace Soiskatel.Mcp.TextCheck;

/// <summary>Проверка письма по правилам рунета: механическое исправляет, остальное возвращает предупреждениями.</summary>
public sealed class TextChecker
{
    private static readonly Regex Invisible = new("[​‌‍⁠﻿­]");
    private static readonly Regex NonBreakingSpace = new("[  ]");
    private static readonly Regex CurlyQuotes = new("[“”„]");
    private static readonly Regex Ellipsis = new("…");
    private static readonly Regex Arrows = new(@"→|←|⇒|⇐|➔|➜|⟶|->|=>|<-");
    private static readonly Regex Participation = new(@"участв\w*|\bучасти[еяию]\w*", RegexOptions.IgnoreCase);
    private static readonly Regex Digit = new(@"\d");

    public TextCheckResult Check(string text, string? platform)
    {
        var fixes = new List<string>();
        var warnings = new List<string>();

        text = Replace(text, Invisible, "", "удалены невидимые символы", fixes);
        text = Replace(text, NonBreakingSpace, " ", "неразрывные пробелы заменены обычными", fixes);

        var (withoutDashes, dashes) = DashRule.Apply(text);
        if (dashes > 0)
            fixes.Add($"тире заменены запятой или дефисом: {dashes}");
        text = withoutDashes;

        text = Replace(text, CurlyQuotes, "\"", "кавычки “”„ заменены на \"", fixes);
        text = Replace(text, Ellipsis, "...", "многоточие заменено тремя точками", fixes);

        if (Arrows.IsMatch(text))
            warnings.Add("есть стрелки: перепишите эти места словами");
        if (Participation.IsMatch(text))
            warnings.Add("есть слово «участие» или его формы: замените активным глаголом (спроектировал, реализовал, ускорил)");
        if (!Digit.IsMatch(text))
            warnings.Add("нет ни одной цифры: добавьте результат с цифрой из профиля");

        var limit = PlatformLimits.For(platform);
        if (text.Length > limit.MaxLength)
            warnings.Add($"длина {text.Length} символов больше {limit.MaxLength} ({limit.Name}): сократите");

        return new TextCheckResult(text, fixes, warnings);
    }

    private static string Replace(string text, Regex regex, string replacement, string description, List<string> fixes)
    {
        var count = regex.Count(text);
        if (count == 0)
            return text;

        fixes.Add($"{description}: {count}");
        return regex.Replace(text, replacement);
    }
}
