namespace Soiskatel.Mcp.Tracker;

/// <summary>Файл applications.md целиком: текст до таблицы, строки таблицы, текст после неё.</summary>
public sealed record TrackerDocument(
    IReadOnlyList<string> Preamble,
    IReadOnlyList<TrackerRow> Rows,
    IReadOnlyList<string> Trailing)
{
    /// <summary>Перевод строки, которым был записан исходный файл («\n» или «\r\n»).</summary>
    public string NewLine { get; init; } = "\n";

    public static TrackerDocument Empty { get; } = new(["# Отклики", ""], [], []);

    public IEnumerable<JobApplication> Applications => Rows.OfType<ParsedRow>().Select(r => r.Application);

    public IReadOnlyList<RawRow> Problems => Rows.OfType<RawRow>().ToList();
}
