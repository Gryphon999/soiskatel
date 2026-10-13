using Soiskatel.Mcp.TextCheck;

namespace Soiskatel.Mcp.Tests.TextCheck;

public class DashRuleTests
{
    [Theory]
    [InlineData("9–18", "9-18")]
    [InlineData("9 — 18", "9-18")]
    [InlineData("2020--2024", "2020-2024")]
    public void Dash_between_numbers_becomes_hyphen(string input, string expected) =>
        Assert.Equal(expected, DashRule.Apply(input).Text);

    [Theory]
    [InlineData("Москва — Ереван", "Москва, Ереван")]
    [InlineData("опыт—главное", "опыт, главное")]
    [InlineData("стек – C# и SQL", "стек, C# и SQL")]
    [InlineData("быстро -- надёжно", "быстро, надёжно")]
    public void Dash_between_words_becomes_comma(string input, string expected) =>
        Assert.Equal(expected, DashRule.Apply(input).Text);

    [Fact]
    public void Comma_before_dash_is_not_doubled() =>
        Assert.Equal("итак, начнём", DashRule.Apply("итак, — начнём").Text);

    [Fact]
    public void Dash_at_line_start_becomes_list_marker() =>
        Assert.Equal("строка\n- пункт", DashRule.Apply("строка\n— пункт").Text);

    [Fact]
    public void Ordinary_hyphen_is_untouched()
    {
        var (text, replacements) = DashRule.Apply("ASP.NET Core, кто-то, 9-18");

        Assert.Equal("ASP.NET Core, кто-то, 9-18", text);
        Assert.Equal(0, replacements);
    }

    [Fact]
    public void Counts_every_replacement() =>
        Assert.Equal(3, DashRule.Apply("А — Б, 1–2, В -- Г").Replacements);

    [Fact]
    public void Dash_does_not_merge_across_line_breaks() =>
        Assert.Equal("10\n- 20", DashRule.Apply("10\n— 20").Text);

    [Fact]
    public void Technical_double_dash_is_untouched()
    {
        var (text, replacements) = DashRule.Apply("флаг --verbose");

        Assert.Equal("флаг --verbose", text);
        Assert.Equal(0, replacements);
    }

    [Fact]
    public void Triple_dash_is_untouched()
    {
        var (text, replacements) = DashRule.Apply("---");

        Assert.Equal("---", text);
        Assert.Equal(0, replacements);
    }

    [Fact]
    public void Double_dash_at_line_start_becomes_list_marker() =>
        Assert.Equal("строка\n- пункт", DashRule.Apply("строка\n-- пункт").Text);
}
