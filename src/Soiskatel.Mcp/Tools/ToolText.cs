using System.Text;
using Soiskatel.Mcp.TextCheck;
using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tools;

/// <summary>Ответы инструментов: короткий русский текст, который Claude пересказывает человеку.</summary>
internal static class ToolText
{
    public static string Format(TextCheckResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Исправленный текст:");
        sb.AppendLine(result.Text);
        sb.AppendLine();
        AppendList(sb, "Исправлено", result.Fixes, "Исправлений нет.");
        AppendList(sb, "Предупреждения", result.Warnings, "Предупреждений нет.");
        return sb.ToString().TrimEnd();
    }

    public static string Format(AddResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Добавлено: {Describe(result.Added)}.");
        AppendList(sb, "Раньше в эту компанию", result.EarlierInSameCompany.Select(Describe).ToList(), empty: "");
        AppendProblems(sb, result.Problems);
        return sb.ToString().TrimEnd();
    }

    public static string Format(UpdateResult result)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Обновлено: {Describe(result.Updated)}.");
        sb.AppendLine($"История: {HistoryText.Format(result.Updated.Application.History)}");
        AppendProblems(sb, result.Problems);
        return sb.ToString().TrimEnd();
    }

    public static string Format(FindResult result, string company)
    {
        var sb = new StringBuilder();
        AppendList(sb, $"Отклики в «{company}»", result.Matches.Select(Describe).ToList(), empty: $"Откликов в «{company}» нет.");
        AppendList(sb, "Возможно, та же компания", result.PossibleMatches.Select(Describe).ToList(), empty: "");
        AppendProblems(sb, result.Problems);
        return sb.ToString().TrimEnd();
    }

    public static string Format(StatsResult result)
    {
        var f = result.Funnel;
        var period = (result.From, result.To) switch
        {
            ({ } from, { } to) => $"с {IsoDate.Format(from)} по {IsoDate.Format(to)}",
            ({ } from, null) => $"с {IsoDate.Format(from)}",
            (null, { } to) => $"по {IsoDate.Format(to)}",
            _ => "за всё время",
        };

        var sb = new StringBuilder();
        sb.AppendLine($"Воронка {period}:");
        sb.AppendLine($"Отправлено: {f.Sent}");
        sb.AppendLine($"Ответили: {f.Responded} ({Percent(f.Responded, f.Sent)})");
        sb.AppendLine($"Собеседования: {f.Interviews} ({Percent(f.Interviews, f.Sent)})");
        sb.AppendLine($"Офферы: {f.Offers} ({Percent(f.Offers, f.Sent)})");
        AppendList(
            sb,
            $"Без ответа {Funnel.NoResponseDays} дней и больше",
            result.NoResponse.Select(n => $"{Describe(n)}, последняя запись {IsoDate.Format(n.Application.LastChange)}").ToList(),
            empty: $"Без ответа {Funnel.NoResponseDays} дней и больше: нет.");
        AppendProblems(sb, result.Problems);
        return sb.ToString().TrimEnd();
    }

    private static string Describe(NumberedApplication n) =>
        $"№{n.RowNumber} {n.Application.Company}, {n.Application.Position}, статус «{StatusNames.ToRussian(n.Application.Status)}»";

    private static string Percent(int part, int total) =>
        total == 0 ? "0%" : $"{Math.Round(100.0 * part / total)}%";

    private static void AppendList(StringBuilder sb, string title, IReadOnlyList<string> items, string empty)
    {
        if (items.Count == 0)
        {
            if (empty.Length > 0)
                sb.AppendLine(empty);
            return;
        }

        sb.AppendLine(title + ":");
        foreach (var item in items)
            sb.AppendLine("- " + item);
    }

    private static void AppendProblems(StringBuilder sb, IReadOnlyList<RawRow> problems)
    {
        if (problems.Count == 0)
            return;

        sb.AppendLine("Строки, которые не удалось разобрать (сохранены без изменений, поправьте вручную):");
        foreach (var p in problems)
            sb.AppendLine($"- строка файла {p.LineNumber}: {p.Problem}");
    }
}
