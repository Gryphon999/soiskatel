namespace Soiskatel.Mcp.Tracker;

/// <summary>Одна смена статуса: когда и какой статус стал.</summary>
public sealed record HistoryEntry(DateOnly Date, ApplicationStatus Status);
