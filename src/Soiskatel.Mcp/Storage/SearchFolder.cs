namespace Soiskatel.Mcp.Storage;

/// <summary>Папка поиска работы. Сервер читает и пишет только внутри неё.</summary>
public sealed class SearchFolder
{
    private static readonly StringComparison PathComparison =
        OperatingSystem.IsWindows() || OperatingSystem.IsMacOS()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;

    public SearchFolder(string root) => Root = Path.GetFullPath(root);

    public string Root { get; }

    /// <summary>Папка из SOISKATEL_HOME; если переменная пустая или не раскрылась, рабочая папка процесса.</summary>
    public static string ResolveRoot(string? fromEnvironment, string currentDirectory)
    {
        var useEnvironment = !string.IsNullOrWhiteSpace(fromEnvironment)
            && !fromEnvironment.Contains("${", StringComparison.Ordinal);

        return Path.GetFullPath(useEnvironment ? fromEnvironment! : currentDirectory);
    }

    /// <summary>Полный путь к файлу внутри папки поиска. Полные пути и выход наружу запрещены.</summary>
    public string PathOf(string relativePath)
    {
        if (Path.IsPathRooted(relativePath))
            throw new StorageException($"Нужен путь внутри папки поиска, а не полный путь: {relativePath}");

        var full = Path.GetFullPath(Path.Combine(Root, relativePath));
        var rootWithSeparator = Path.EndsInDirectorySeparator(Root) ? Root : Root + Path.DirectorySeparatorChar;

        if (!full.StartsWith(rootWithSeparator, PathComparison))
            throw new StorageException($"Путь ведёт за пределы папки поиска: {relativePath}");

        return full;
    }

    public void EnsureExists()
    {
        if (!Directory.Exists(Root))
            throw new StorageException($"Папка поиска не найдена: {Root}. Запустите Claude Code в папке поиска работы.");

        if (FindGitAncestor() is not null)
            throw new StorageException(
                $"Папка поиска внутри git-репозитория ({Root}). Личные данные могут попасть в коммит. "
                + "Создайте отдельную папку вне репозиториев и запустите Claude Code в ней.");
    }

    private string? FindGitAncestor()
    {
        for (var dir = new DirectoryInfo(Root); dir is not null; dir = dir.Parent)
        {
            var candidate = Path.Combine(dir.FullName, ".git");
            if (Directory.Exists(candidate) || File.Exists(candidate))
                return dir.FullName;
        }

        return null;
    }
}
