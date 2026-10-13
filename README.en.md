# soiskatel

AI job-search assistant for Claude Code, made for the Russian-speaking job market (hh.ru, Habr Career, GeekJob, staff.am). It analyzes a vacancy, writes an honest cover letter that follows local conventions, and keeps an application tracker. No auto-clicking and no hh.ru API: the tool never acts on behalf of your account.

## Install

    /plugin marketplace add Gryphon999/soiskatel
    /plugin install soiskatel@soiskatel

Optional, needs the .NET 10 SDK (tracker statistics and exact letter checks):

    /plugin install soiskatel-mcp@soiskatel

Skills talk in Russian; letters can be Russian or English. Your data stays in your folder; the optional MCP server's code makes no network calls, which an architecture test enforces (the first `dotnet` run does fetch the server package from NuGet once). MIT license. Not affiliated with HeadHunter.
