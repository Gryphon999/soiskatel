<#
.SYNOPSIS
  Ищет личные данные автора в репозитории. Шаблоны лежат вне репозитория.
.DESCRIPTION
  Без -History проверяет содержимое отслеживаемых файлов. С -History проверяет
  все добавленные и удалённые строки во всей истории, полный текст (заголовок и тело)
  каждого коммита, а также имена и почты авторов и комиттеров.
  Коды выхода: 0 чисто, 1 найдено, 2 нет файла шаблонов или он пуст (fail-closed).
  Найденные значения не печатаются, только номер шаблона и место.
#>
[CmdletBinding()]
param(
    [switch]$History,
    [string]$PatternsFile = (Join-Path $HOME '.soiskatel/private-patterns.txt')
)

$ErrorActionPreference = 'Stop'
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

if (-not (Test-Path -LiteralPath $PatternsFile)) {
    Write-Host "check-private: нет файла шаблонов $PatternsFile. Проверка не может пройти."
    exit 2
}

$patterns = @(Get-Content -LiteralPath $PatternsFile -Encoding utf8 |
    ForEach-Object { $_.Trim() } |
    Where-Object { $_ -and -not $_.StartsWith('#') })

if ($patterns.Count -eq 0) {
    Write-Host "check-private: в $PatternsFile нет ни одного шаблона. Проверка не может пройти."
    exit 2
}

$regexes = @($patterns | ForEach-Object {
    [regex]::new($_, [System.Text.RegularExpressions.RegexOptions]::IgnoreCase)
})
$script:found = 0

function Test-Line([string]$Line, [string]$Where) {
    for ($i = 0; $i -lt $regexes.Count; $i++) {
        if ($regexes[$i].IsMatch($Line)) {
            Write-Host "НАЙДЕНО: шаблон #$($i + 1), $Where"
            $script:found++
        }
    }
}

if ($History) {
    $commit = ''
    $file = ''
    git -c core.quotepath=off log -p --all --no-color --format='commit %h' | ForEach-Object {
        if ($_ -match '^commit ([0-9a-f]+)$') { $commit = $Matches[1]; return }
        if ($_ -match '^\+\+\+ b/(.+)$') { $file = $Matches[1]; return }
        if ($_.StartsWith('+') -or $_.StartsWith('-')) { Test-Line $_ "коммит $commit, файл $file" }
    }
    $metaCommit = ''
    git log --all --format='%h %an <%ae> %cn <%ce>%n%B' | ForEach-Object {
        if ($_ -match '^([0-9a-f]{4,40}) .+ <[^>]*> .+ <[^>]*>$') {
            $metaCommit = $Matches[1]
        }
        Test-Line $_ "метаданные коммита $metaCommit"
    }
}
else {
    git -c core.quotepath=off ls-files | ForEach-Object {
        $path = $_
        if (Test-Path -LiteralPath $path -PathType Leaf) {
            $n = 0
            foreach ($line in [System.IO.File]::ReadLines((Resolve-Path -LiteralPath $path).Path)) {
                $n++
                Test-Line $line "$path, строка $n"
            }
        }
    }
}

if ($script:found -gt 0) {
    Write-Host "check-private: совпадений $($script:found). Уберите личные данные и повторите."
    exit 1
}

Write-Host 'check-private: чисто.'
exit 0
