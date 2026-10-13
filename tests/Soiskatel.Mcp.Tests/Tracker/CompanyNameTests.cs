using Soiskatel.Mcp.Tracker;

namespace Soiskatel.Mcp.Tests.Tracker;

public class CompanyNameTests
{
    [Theory]
    [InlineData("ООО «Ромашка»", "ромашка")]
    [InlineData("Ромашка", "ромашка")]
    [InlineData("РОМАШКА LLC", "ромашка")]
    [InlineData("Ёлка, Inc.", "елка")]
    [InlineData("  Яндекс   Маркет ", "яндекс маркет")]
    [InlineData("АО \"Тинькофф Банк\"", "тинькофф банк")]
    [InlineData("ИП Иванов", "иванов")]
    [InlineData("Bluebird Labs Ltd.", "bluebird labs")]
    [InlineData("Альфа-Банк", "альфа банк")]
    [InlineData("Даосский Код", "даосский код")]
    [InlineData("ООО", "")]
    public void Normalize(string input, string expected) =>
        Assert.Equal(expected, CompanyName.Normalize(input));

    [Fact]
    public void Same_company_in_different_spelling() =>
        Assert.True(CompanyName.AreSame("ООО «Ромашка»", "ромашка"));

    [Fact]
    public void Empty_names_are_not_the_same() =>
        Assert.False(CompanyName.AreSame("ООО", "АО"));

    [Theory]
    [InlineData("Яндекс Маркет", "Яндекс", true)]
    [InlineData("Яндекс", "Яндекс Маркет", true)]
    [InlineData("Ромашка", "ООО Ромашка", false)]
    [InlineData("Ромашка", "Ромашкин", false)]
    public void Possible_match_only_for_word_prefix(string a, string b, bool expected) =>
        Assert.Equal(expected, CompanyName.IsPossibleMatch(a, b));
}
