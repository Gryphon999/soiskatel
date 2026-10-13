using System.Text.RegularExpressions;

namespace Soiskatel.Mcp.TextCheck;

/// <summary>
/// Убирает длинное «—», короткое «–» тире и «--».
/// Между числами ставит дефис («9–18» станет «9-18»), в начале строки маркер списка «- »,
/// между словами запятую («Москва — Ереван» станет «Москва, Ереван»).
/// </summary>
public static class DashRule
{
    /// <returns>Исправленный текст и число замен.</returns>
    public static (string Text, int Replacements) Apply(string text)
    {
        var count = 0;
        var result = Regex.Replace(text, @"(\d)[ \t]*(?:—|–|--)[ \t]*(\d)", m => { count++; return $"{m.Groups[1].Value}-{m.Groups[2].Value}"; });
        result = Regex.Replace(result, @"(?m)^([ \t]*)(?:[—–][ \t]*|--[ \t]+)", m => { count++; return m.Groups[1].Value + "- "; });
        result = Regex.Replace(result, @",?[ \t]*[—–][ \t]*|,?[ \t]+--[ \t]+", _ => { count++; return ", "; });
        return (result, count);
    }
}
