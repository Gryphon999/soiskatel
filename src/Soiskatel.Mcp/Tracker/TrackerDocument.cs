namespace Soiskatel.Mcp.Tracker;

/// <summary>Файл applications.md целиком: текст до таблицы, строки таблицы, текст после неё.</summary>
public sealed record TrackerDocument(
    IReadOnlyList<string> Preamble,
    IReadOnlyList<TrackerRow> Rows,
    IReadOnlyList<string> Trailing)
{
    /// <summary>Перевод строки, которым был записан исходный файл («\n» или «\r\n»).</summary>
    public string NewLine { get; init; } = "\n";

    /// <summary>
    /// Ложь означает: файл непустой, но в нём нет таблицы с заголовком формата soiskatel.
    /// Пустой, пробельный или отсутствующий файл даёт true (можно начать таблицу заново).
    /// </summary>
    public bool HasTable { get; init; } = true;

    public static TrackerDocument Empty { get; } = new(["# Отклики", ""], [], []);

    public IEnumerable<JobApplication> Applications => Rows.OfType<ParsedRow>().Select(r => r.Application);

    public IReadOnlyList<RawRow> Problems => Rows.OfType<RawRow>().ToList();
}
