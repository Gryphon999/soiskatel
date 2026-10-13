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
}
