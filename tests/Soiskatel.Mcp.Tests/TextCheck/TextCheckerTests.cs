using Soiskatel.Mcp.TextCheck;

namespace Soiskatel.Mcp.Tests.TextCheck;

public class TextCheckerTests
{
    private readonly TextChecker _checker = new();

    [Fact]
    public void Removes_invisible_characters()
    {
        var result = _checker.Check("при​вет­ 1", platform: null);

        Assert.Equal("привет 1", result.Text);
        Assert.Contains(result.Fixes, f => f.StartsWith("удалены невидимые символы"));
    }

    [Fact]
    public void Replaces_non_breaking_spaces()
    {
        var result = _checker.Check("10 000 строк", platform: null);

        Assert.Equal("10 000 строк", result.Text);
        Assert.Contains(result.Fixes, f => f.StartsWith("неразрывные пробелы"));
    }

    [Fact]
    public void Applies_dash_rule()
    {
        var result = _checker.Check("Москва — Ереван, 9–18", platform: null);

        Assert.Equal("Москва, Ереван, 9-18", result.Text);
        Assert.Contains(result.Fixes, f => f.StartsWith("тире заменены"));
    }

    [Fact]
    public void Replaces_curly_quotes()
    {
        var result = _checker.Check("проект “TaskBoard” 1", platform: null);

        Assert.Equal("проект \"TaskBoard\" 1", result.Text);
    }

    [Fact]
    public void Keeps_russian_guillemets()
    {
        var result = _checker.Check("студия «Пример» 1", platform: null);

        Assert.Equal("студия «Пример» 1", result.Text);
        Assert.Empty(result.Fixes);
    }

    [Fact]
    public void Replaces_ellipsis()
    {
        var result = _checker.Check("итак… 1", platform: null);

        Assert.Equal("итак... 1", result.Text);
    }

    [Theory]
    [InlineData("C# -> SQL 1")]
    [InlineData("C# → SQL 1")]
    [InlineData("C# => SQL 1")]
    public void Warns_about_arrows(string text)
    {
        var result = _checker.Check(text, platform: null);

        Assert.Contains(result.Warnings, w => w.StartsWith("есть стрелки"));
    }

    [Theory]
    [InlineData("Принимал участие в проекте 1")]
    [InlineData("Участвовал в разработке 1")]
    [InlineData("участвовала в релизе 1")]
    [InlineData("Хочу поучаствовать в проекте 1")]
    public void Warns_about_participation(string text)
    {
        var result = _checker.Check(text, platform: null);

        Assert.Contains(result.Warnings, w => w.StartsWith("есть слово «участие»"));
    }

    [Fact]
    public void Does_not_warn_about_participant()
    {
        var result = _checker.Check("участник команды из 5 человек", platform: null);

        Assert.DoesNotContain(result.Warnings, w => w.StartsWith("есть слово «участие»"));
    }

    [Fact]
    public void Warns_when_no_digits()
    {
        var result = _checker.Check("Опытный разработчик", platform: null);

        Assert.Contains(result.Warnings, w => w.StartsWith("нет ни одной цифры"));
    }

    [Fact]
    public void Clean_text_has_no_fixes_and_no_warnings()
    {
        var result = _checker.Check("Ускорил отчёт в 15 раз.", platform: "hh.ru");

        Assert.Equal("Ускорил отчёт в 15 раз.", result.Text);
        Assert.Empty(result.Fixes);
        Assert.Empty(result.Warnings);
    }

    [Fact]
    public void Staff_am_limit_is_500()
    {
        var result = _checker.Check(new string('а', 500) + "1", platform: "staff.am");

        Assert.Contains(result.Warnings, w => w.Contains("больше 500"));
    }

    [Fact]
    public void Default_limit_is_2000()
    {
        Assert.DoesNotContain(_checker.Check(new string('а', 1999) + "1", platform: "hh.ru").Warnings, w => w.StartsWith("длина"));
        Assert.Contains(_checker.Check(new string('а', 2000) + "1", platform: "hh.ru").Warnings, w => w.StartsWith("длина"));
    }
}
