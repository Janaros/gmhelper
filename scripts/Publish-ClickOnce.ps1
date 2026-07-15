<#
Builds and publishes GMHelper.App as a ClickOnce package into <repo>\publish\.

ClickOnce's manifest-generation MSBuild tasks (GenerateTrustInfo, GenerateDeploymentManifest, ...)
are not supported by the .NET SDK's cross-platform `dotnet` CLI (MSB4803) - they require the
full-framework MSBuild that ships with Visual Studio. This script locates that MSBuild via
vswhere and drives it directly instead of `dotnet publish`.

The ApplicationVersion's first three segments always mirror the assembly <Version> in
GMHelper.App.csproj, so bumping that one property (already required for every shipped change,
see CLAUDE.md) is enough to make ClickOnce recognize a new version - the revision segment stays 0.

Usage: pwsh scripts/Publish-ClickOnce.ps1
#>

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$csproj = Join-Path $repoRoot "src\GMHelper.App\GMHelper.App.csproj"

$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    throw "vswhere.exe not found at '$vswhere'. Visual Studio (or Build Tools) with the ClickOnce publishing component is required."
}

$vsInstallPath = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -property installationPath
if (-not $vsInstallPath) {
    throw "No Visual Studio installation with MSBuild found."
}

$msbuild = Join-Path $vsInstallPath "MSBuild\Current\Bin\amd64\MSBuild.exe"
if (-not (Test-Path $msbuild)) {
    throw "MSBuild.exe not found under '$vsInstallPath'."
}

$version = (dotnet msbuild $csproj -getProperty:Version -nologo).Trim()
if (-not $version) {
    throw "Could not read <Version> from $csproj."
}
$applicationVersion = "$version.0"

$publishDir = Join-Path $repoRoot "publish\"

Write-Host "Publishing GMHelper v$version (ClickOnce ApplicationVersion $applicationVersion)..."

# Configuration=Release and the publish paths are passed as command-line properties (not left
# to the .pubxml alone) because MSBuild evaluates ProjectReferences' configuration before the
# .pubxml's <Configuration> is imported - relying on the .pubxml alone silently pulled in Debug
# DLLs from the referenced Core/Data/Services projects during testing.
& $msbuild $csproj `
    -t:Publish -restore `
    -p:Configuration=Release `
    -p:PublishProfile=ClickOnce `
    -p:PublishDir=$publishDir `
    -p:PublishUrl=$publishDir `
    -p:ApplicationVersion=$applicationVersion

if ($LASTEXITCODE -ne 0) {
    throw "ClickOnce publish failed with exit code $LASTEXITCODE."
}

Write-Host "Published to $publishDir"
