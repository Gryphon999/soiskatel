<#
.SYNOPSIS
  Записывает одну версию во все пять мест: csproj, server.json, .mcp.json и оба plugin.json.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidatePattern('^\d+\.\d+\.\d+(-[0-9A-Za-z.]+)?$')]
    [string]$Version
)

$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot -Parent

function Set-InFile([string]$RelativePath, [string]$Pattern, [string]$Replacement) {
    $path = Join-Path $root $RelativePath
    $text = [System.IO.File]::ReadAllText($path)
    if (-not [regex]::IsMatch($text, $Pattern)) {
        throw "В $RelativePath не найден шаблон $Pattern"
    }
    $updated = [regex]::Replace($text, $Pattern, $Replacement)
    [System.IO.File]::WriteAllText($path, $updated, [System.Text.UTF8Encoding]::new($false))
}

Set-InFile 'src/Soiskatel.Mcp/Soiskatel.Mcp.csproj' '<Version>[^<]+</Version>' "<Version>$Version</Version>"
Set-InFile 'src/Soiskatel.Mcp/.mcp/server.json' '"version": "[^"]+"' "`"version`": `"$Version`""
Set-InFile 'plugins/soiskatel-mcp/.mcp.json' 'Soiskatel\.Mcp@[^"]+' "Soiskatel.Mcp@$Version"
Set-InFile 'plugins/soiskatel/.claude-plugin/plugin.json' '"version": "[^"]+"' "`"version`": `"$Version`""
Set-InFile 'plugins/soiskatel-mcp/.claude-plugin/plugin.json' '"version": "[^"]+"' "`"version`": `"$Version`""

Write-Host "Версия $Version записана в 5 файлов."
