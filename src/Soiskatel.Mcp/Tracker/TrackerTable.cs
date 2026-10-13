using System.Text;
using System.Text.RegularExpressions;

namespace Soiskatel.Mcp.Tracker;

/// <summary>Чтение и запись applications.md. Строки, которые не удалось разобрать, сохраняются как есть.</summary>
public static class TrackerTable
{
    private static readonly Regex Separator = new(@"^\s*\|(\s*:?-{3,}:?\s*\|)+\s*$");

    public static IReadOnlyList<string> Columns { get; } =
        ["Дата", "Компания", "Вакансия", "Площадка", "Ссылка", "Статус", "История", "Заметки"];

    public static string HeaderLine { get; } = "| " + string.Join(" | ", Columns) + " |";

    public static string SeparatorLine { get; } = "|" + string.Concat(Enumerable.Repeat("---|", Columns.Count));

    public static TrackerDocument Parse(string content)
    {
        var isBlank = string.IsNullOrWhiteSpace(content);

        if (content.Length > 0 && content[0] == '﻿')
            content = content[1..];

        var newLine = content.Contains("\r\n") ? "\r\n" : "\n";

        var lines = content.Replace("\r\n", "\n").Split('\n');
        if (lines.Length > 0 && lines[^1].Length == 0)
            lines = lines[..^1];

        var header = Array.FindIndex(lines, IsHeader);
        if (header < 0)
            return new TrackerDocument(lines, [], []) { NewLine = newLine, HasTable = isBlank };

        var i = header + 1;
        if (i < lines.Length && Separator.IsMatch(lines[i]))
            i++;

        var rows = new List<TrackerRow>();
        for (; i < lines.Length && lines[i].TrimStart().StartsWith('|'); i++)
            rows.Add(ParseRow(lines[i], lineNumber: i + 1));

        return new TrackerDocument(lines[..header], rows, lines[i..]) { NewLine = newLine };
    }

    public static string Write(TrackerDocument document)
    {
        var newLine = document.NewLine;
        var sb = new StringBuilder();
        foreach (var line in document.Preamble)
            sb.Append(line).Append(newLine);

        sb.Append(HeaderLine).Append(newLine);
        sb.Append(SeparatorLine).Append(newLine);

        foreach (var row in document.Rows)
        {
            var line = row switch
            {
                ParsedRow parsed => FormatRow(parsed.Application),
                RawRow raw => raw.Line,
                _ => throw new InvalidOperationException($"Неизвестный тип строки: {row.GetType().Name}"),
            };
            sb.Append(line).Append(newLine);
        }

        foreach (var line in document.Trailing)
            sb.Append(line).Append(newLine);

        return sb.ToString();
    }

    public static string FormatRow(JobApplication application)
    {
        string[] cells =
        [
            IsoDate.Format(application.Date),
            application.Company,
            application.Position,
            application.Platform,
            application.Link,
            StatusNames.ToRussian(application.Status),
            HistoryText.Format(application.History),
            application.Notes,
        ];

        return "| " + string.Join(" | ", cells.Select(EscapeCell)) + " |";
    }

    private static bool IsHeader(string line) => SplitCells(line).SequenceEqual(Columns);

    private static TrackerRow ParseRow(string line, int lineNumber)
    {
        var c = SplitCells(line);
        if (c.Count != Columns.Count)
            return new RawRow(lineNumber, line, $"ожидалось {Columns.Count} колонок, найдено {c.Count}");
        if (!IsoDate.TryParse(c[0], out var date))
            return new RawRow(lineNumber, line, $"дата «{c[0]}» не в формате ГГГГ-ММ-ДД");
        if (string.IsNullOrWhiteSpace(c[1]))
            return new RawRow(lineNumber, line, "не указана компания");
        if (!StatusNames.TryParse(c[5], out var status))
            return new RawRow(lineNumber, line, $"неизвестный статус «{c[5]}»");
        if (!HistoryText.TryParse(c[6], out var history))
            return new RawRow(lineNumber, line, "история не в формате «ГГГГ-ММ-ДД статус; ГГГГ-ММ-ДД статус»");

        return new ParsedRow(new JobApplication(date, c[1], c[2], c[3], c[4], status, history, c[7]));
    }

    private static List<string> SplitCells(string line)
    {
        var trimmed = line.Trim();
        var cells = new List<string>();
        if (!trimmed.StartsWith('|'))
            return cells;

        var current = new StringBuilder();
        for (var i = 1; i < trimmed.Length; i++)
        {
            var ch = trimmed[i];
            if (ch == '\\' && i + 1 < trimmed.Length && trimmed[i + 1] == '|')
            {
                current.Append('|');
                i++;
            }
            else if (ch == '|')
            {
                cells.Add(current.ToString().Trim());
                current.Clear();
            }
            else
            {
                current.Append(ch);
            }
        }

        if (current.ToString().Trim().Length > 0)
            cells.Add(current.ToString().Trim());

        return cells;
    }

    private static string EscapeCell(string value) =>
        value.Replace("\r", " ").Replace("\n", " ").Replace("|", "\\|").Trim();
}
