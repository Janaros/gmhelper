<#
Publishes GMHelper.App via ClickOnce and deploys the result to the gh-pages branch on GitHub,
which GitHub Pages serves at https://janaros.github.io/gmhelper/.

Runs the publish in a disposable local clone with core.autocrlf disabled and a "* -text -diff"
.gitattributes. ClickOnce's .manifest/.application files are plain-text XML with embedded
SHA-256 hashes of their own exact bytes; if git normalizes their line endings on commit (the
default on Windows with autocrlf=true), the hash embedded by the ClickOnce publish step no
longer matches what gets served, and every client fails activation with "Die Gultigkeit der
Anwendung konnte nicht uberpruft werden" even though the files look fine to the eye. This bit
us on the very first deploy - see git history of this file for details.

Usage: pwsh scripts/Deploy-GhPages.ps1
#>

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot "Publish-ClickOnce.ps1")

$publishDir = Join-Path $repoRoot "publish"
if (-not (Test-Path $publishDir)) {
    throw "Publish output not found at $publishDir."
}

$remoteUrl = (git -C $repoRoot remote get-url origin).Trim()

$scratch = Join-Path ([System.IO.Path]::GetTempPath()) "gmhelper-gh-pages-deploy"
if (Test-Path $scratch) {
    Remove-Item -Recurse -Force $scratch
}

git clone -q $remoteUrl $scratch
Push-Location $scratch
try {
    git config user.name "Janaros"
    git config user.email "markus.schuer@googlemail.com"
    git config core.autocrlf false
    git checkout --orphan gh-pages
    git rm -rf -q .
    Set-Content -Path ".gitattributes" -Value "* -text -diff" -NoNewline:$false -Encoding utf8

    Copy-Item -Path (Join-Path $publishDir "*") -Destination $scratch -Recurse -Force

    git add -A
    $version = (dotnet msbuild (Join-Path $repoRoot "src\GMHelper.App\GMHelper.App.csproj") -getProperty:Version -nologo).Trim()
    git commit -q -m "Deploy GMHelper $version ClickOnce package"
    git push --force origin gh-pages
}
finally {
    Pop-Location
}

Remove-Item -Recurse -Force $scratch

Write-Host "Deployed to gh-pages. Live at https://janaros.github.io/gmhelper/ (GitHub Pages build may take a minute)."
