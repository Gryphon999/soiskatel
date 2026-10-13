using Soiskatel.Mcp.Storage;
using Soiskatel.Mcp.Tests.TestSupport;

namespace Soiskatel.Mcp.Tests.Storage;

public class SearchFolderTests
{
    [Fact]
    public void PathOf_returns_file_inside_root()
    {
        using var temp = new TempFolder();
        var folder = new SearchFolder(temp.Root);

        Assert.Equal(Path.Combine(temp.Root, "applications.md"), folder.PathOf("applications.md"));
    }

    [Theory]
    [InlineData("../outside.md")]
    [InlineData("letters/../../outside.md")]
    public void PathOf_rejects_paths_leading_outside(string relative)
    {
        using var temp = new TempFolder();
        var folder = new SearchFolder(temp.Root);

        var ex = Assert.Throws<StorageException>(() => folder.PathOf(relative));
        Assert.Contains("за пределы папки поиска", ex.Message);
    }

    [Fact]
    public void PathOf_rejects_rooted_path()
    {
        using var temp = new TempFolder();
        var folder = new SearchFolder(temp.Root);

        Assert.Throws<StorageException>(() => folder.PathOf(Path.Combine(temp.Root, "applications.md")));
    }

    [Fact]
    public void EnsureExists_throws_with_hint_when_folder_is_missing()
    {
        var missing = Path.Combine(Path.GetTempPath(), "soiskatel-tests", "missing-" + Guid.NewGuid().ToString("N"));
        var folder = new SearchFolder(missing);

        var ex = Assert.Throws<StorageException>(folder.EnsureExists);
        Assert.Contains("Запустите Claude Code в папке поиска работы", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("${CLAUDE_PROJECT_DIR}")]
    public void ResolveRoot_falls_back_to_current_directory(string? fromEnvironment)
    {
        using var temp = new TempFolder();

        Assert.Equal(Path.GetFullPath(temp.Root), SearchFolder.ResolveRoot(fromEnvironment, temp.Root));
    }

    [Fact]
    public void ResolveRoot_prefers_environment_value()
    {
        using var fromEnv = new TempFolder();
        using var cwd = new TempFolder();

        Assert.Equal(Path.GetFullPath(fromEnv.Root), SearchFolder.ResolveRoot(fromEnv.Root, cwd.Root));
    }
}
