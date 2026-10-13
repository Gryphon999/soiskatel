namespace Soiskatel.Mcp.TextCheck;

/// <summary>Исправленный текст, что исправлено автоматически и о чём нужно подумать человеку.</summary>
public sealed record TextCheckResult(string Text, IReadOnlyList<string> Fixes, IReadOnlyList<string> Warnings);
