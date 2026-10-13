using System.Reflection;
using NetArchTest.Rules;

namespace Soiskatel.Mcp.Tests;

public class ArchitectureTests
{
    private static readonly Assembly Server = Assembly.Load("Soiskatel.Mcp");

    [Fact]
    public void Server_types_do_not_use_network()
    {
        var result = Types.InAssembly(Server)
            .ShouldNot()
            .HaveDependencyOnAny("System.Net.Http", "System.Net.Sockets", "System.Net.WebClient", "System.Net.WebRequest")
            .GetResult();

        Assert.True(result.IsSuccessful, "Сетевые зависимости: " + string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_does_not_depend_on_mcp()
    {
        var result = Types.InAssembly(Server)
            .That().ResideInNamespace("Soiskatel.Mcp.Tracker")
            .Or().ResideInNamespace("Soiskatel.Mcp.TextCheck")
            .Or().ResideInNamespace("Soiskatel.Mcp.Storage")
            .ShouldNot().HaveDependencyOn("ModelContextProtocol")
            .GetResult();

        Assert.True(result.IsSuccessful, "Зависят от MCP: " + string.Join(", ", result.FailingTypeNames ?? []));
    }
}
