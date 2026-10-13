namespace Soiskatel.Mcp.Storage;

/// <summary>Ошибка работы с папкой поиска; текст показывается пользователю.</summary>
public sealed class StorageException(string message) : Exception(message);
