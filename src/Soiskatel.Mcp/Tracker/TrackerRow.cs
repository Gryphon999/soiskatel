namespace Soiskatel.Mcp.Tracker;

/// <summary>Строка таблицы: разобранный отклик или строка, которую разобрать не удалось.</summary>
public abstract record TrackerRow;

public sealed record ParsedRow(JobApplication Application) : TrackerRow;

/// <summary>Строка, которую не удалось разобрать. Хранится и записывается обратно без изменений.</summary>
public sealed record RawRow(int LineNumber, string Line, string Problem) : TrackerRow;
