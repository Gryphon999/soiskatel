namespace Soiskatel.Mcp.Tracker;

/// <summary>Ошибка операции трекера; текст показывается пользователю.</summary>
public sealed class TrackerException(string message) : Exception(message);
