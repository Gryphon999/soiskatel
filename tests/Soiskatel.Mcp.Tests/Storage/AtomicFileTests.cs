using Soiskatel.Mcp.Storage;
using Soiskatel.Mcp.Tests.TestSupport;

namespace Soiskatel.Mcp.Tests.Storage;

public class AtomicFileTests
{
    [Fact]
    public void First_write_creates_file_without_backup()
    {
        using var temp = new TempFolder();
        var path = temp.File("applications.md");

        AtomicFile.WriteAllText(path, "первая версия");

        Assert.Equal("первая версия", File.ReadAllText(path));
        Assert.False(File.Exists(path + ".bak"));
        Assert.False(File.Exists(path + ".tmp"));
        Assert.Empty(Directory.GetFiles(temp.Root, "*.tmp"));
    }

    [Fact]
    public void Second_write_keeps_previous_version_in_bak()
    {
        using var temp = new TempFolder();
        var path = temp.File("applications.md");

        AtomicFile.WriteAllText(path, "первая версия");
        AtomicFile.WriteAllText(path, "вторая версия");

        Assert.Equal("вторая версия", File.ReadAllText(path));
        Assert.Equal("первая версия", File.ReadAllText(path + ".bak"));
        Assert.False(File.Exists(path + ".tmp"));
    }

    [Fact]
    public void Writes_utf8_without_bom()
    {
        using var temp = new TempFolder();
        var path = temp.File("applications.md");

        AtomicFile.WriteAllText(path, "я");

        Assert.Equal(new byte[] { 0xD1, 0x8F }, File.ReadAllBytes(path));
    }

    [Fact]
    public void ReadAllTextOrNull_returns_null_for_missing_file()
    {
        using var temp = new TempFolder();

        Assert.Null(AtomicFile.ReadAllTextOrNull(temp.File("missing.md")));
    }

    [Fact]
    public void Repeated_writes_leave_no_leftover_tmp_files()
    {
        using var temp = new TempFolder();
        var path = temp.File("applications.md");

        for (var i = 0; i < 20; i++)
            AtomicFile.WriteAllText(path, $"версия {i}");

        Assert.Equal("версия 19", File.ReadAllText(path));
        Assert.Empty(Directory.GetFiles(temp.Root, "*.tmp"));
    }

    [Fact]
    public void Failed_write_does_not_leave_a_temp_file_behind()
    {
        using var temp = new TempFolder();
        // Папка вместо файла по целевому пути: File.Move/File.Replace упадёт.
        var path = temp.File("blocked");
        Directory.CreateDirectory(path);

        Assert.ThrowsAny<IOException>(() => AtomicFile.WriteAllText(path, "содержимое"));

        Assert.Empty(Directory.GetFiles(temp.Root, "*.tmp"));
    }
}
