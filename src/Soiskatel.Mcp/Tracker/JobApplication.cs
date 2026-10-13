namespace Soiskatel.Mcp.Tracker;

/// <summary>Один отклик: строка таблицы applications.md.</summary>
public sealed record JobApplication(
    DateOnly Date,
    string Company,
    string Position,
    string Platform,
    string Link,
    ApplicationStatus Status,
    IReadOnlyList<HistoryEntry> History,
    string Notes)
{
    /// <summary>Дата последней смены статуса.</summary>
    public DateOnly LastChange => History.Count > 0 ? History[^1].Date : Date;

    /// <summary>Был ли у отклика этот статус хоть раз.</summary>
    public bool Reached(ApplicationStatus status) => History.Any(h => h.Status == status);
}
