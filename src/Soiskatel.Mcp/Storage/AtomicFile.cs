using System.Text;

namespace Soiskatel.Mcp.Storage;

/// <summary>Запись «всё или ничего»: сначала временный файл, потом подмена. Прошлая версия остаётся в .bak.</summary>
public static class AtomicFile
{
    private static readonly UTF8Encoding Utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);

    public static void WriteAllText(string path, string content)
    {
        var temp = path + ".tmp";
        File.WriteAllText(temp, content, Utf8NoBom);

        if (File.Exists(path))
            File.Replace(temp, path, path + ".bak", ignoreMetadataErrors: true);
        else
            File.Move(temp, path);
    }

    public static string? ReadAllTextOrNull(string path) =>
        File.Exists(path) ? File.ReadAllText(path, Encoding.UTF8) : null;
}
