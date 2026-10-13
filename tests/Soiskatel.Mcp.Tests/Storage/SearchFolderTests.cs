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

    [Fact]
    public void EnsureExists_passes_for_ordinary_temp_folder()
    {
        using var temp = new TempFolder();
        var folder = new SearchFolder(temp.Root);

        folder.EnsureExists();
    }

    [Fact]
    public void EnsureExists_throws_when_folder_itself_has_dot_git()
    {
        using var temp = new TempFolder();
        Directory.CreateDirectory(Path.Combine(temp.Root, ".git"));
        var folder = new SearchFolder(temp.Root);

        var ex = Assert.Throws<StorageException>(folder.EnsureExists);
        Assert.Contains("git-репозитория", ex.Message);
        Assert.Contains(temp.Root, ex.Message);
    }

    [Fact]
    public void EnsureExists_throws_when_parent_has_dot_git()
    {
        using var temp = new TempFolder();
        Directory.CreateDirectory(Path.Combine(temp.Root, ".git"));
        var child = Path.Combine(temp.Root, "poisk-raboty");
        Directory.CreateDirectory(child);
        var folder = new SearchFolder(child);

        var ex = Assert.Throws<StorageException>(folder.EnsureExists);
        Assert.Contains("git-репозитория", ex.Message);
    }

    [Fact]
    public void EnsureExists_throws_when_dot_git_is_a_file_worktree_marker()
    {
        using var temp = new TempFolder();
        File.WriteAllText(Path.Combine(temp.Root, ".git"), "gitdir: ../somewhere");
        var folder = new SearchFolder(temp.Root);

        Assert.Throws<StorageException>(folder.EnsureExists);
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
