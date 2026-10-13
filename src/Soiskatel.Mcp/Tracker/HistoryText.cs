namespace Soiskatel.Mcp.Tracker;

/// <summary>Колонка «История»: «ГГГГ-ММ-ДД статус; ГГГГ-ММ-ДД статус».</summary>
public static class HistoryText
{
    public static string Format(IEnumerable<HistoryEntry> entries) =>
        string.Join("; ", entries.Select(e => $"{IsoDate.Format(e.Date)} {StatusNames.ToRussian(e.Status)}"));

    public static bool TryParse(string text, out IReadOnlyList<HistoryEntry> entries)
    {
        var list = new List<HistoryEntry>();
        foreach (var part in text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var space = part.IndexOf(' ');
            if (space < 0
                || !IsoDate.TryParse(part[..space], out var date)
                || !StatusNames.TryParse(part[(space + 1)..], out var status))
            {
                entries = [];
                return false;
            }

            list.Add(new HistoryEntry(date, status));
        }

        entries = list;
        return list.Count > 0;
    }
}
