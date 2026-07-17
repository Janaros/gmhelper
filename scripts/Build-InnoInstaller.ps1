<#
Builds a plain (non-ClickOnce) self-contained win-x64 publish of GMHelper.App and packages it
with Inno Setup into a standalone Setup.exe under <repo>\dist\.

Uses a classic installer instead of ClickOnce because ClickOnce's client-side activation
(System.Deployment.Application) proved unreliable during testing - it repeatedly failed with
opaque errors unrelated to our manifest/hosting (which were independently verified correct),
including a case tied to non-ASCII characters in the local Windows profile path. Inno Setup
just packages the already-known-working self-contained executable directly, without going
through that activation stack at all.

Requires Inno Setup 6 (ISCC.exe) - install via: winget install JRSoftware.InnoSetup

Usage: pwsh scripts/Build-InnoInstaller.ps1
#>

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$csproj = Join-Path $repoRoot "src\GMHelper.App\GMHelper.App.csproj"
$publishDir = Join-Path $repoRoot "publish-inno"
$issFile = Join-Path $repoRoot "installer\GMHelper.iss"

$iscc = Get-ChildItem -Path "C:\", "D:\" -Filter "ISCC.exe" -Recurse -ErrorAction SilentlyContinue -Depth 4 |
    Select-Object -First 1 -ExpandProperty FullName
if (-not $iscc) {
    throw "ISCC.exe (Inno Setup compiler) nicht gefunden. Installieren via: winget install JRSoftware.InnoSetup"
}

$version = (dotnet msbuild $csproj -getProperty:Version -nologo).Trim()
if (-not $version) {
    throw "Could not read <Version> from $csproj."
}

Write-Host "Publishing GMHelper v$version (self-contained win-x64, Release)..."

if (Test-Path $publishDir) {
    Remove-Item -Recurse -Force $publishDir
}

# Embed the Syncfusion license key (gitignored local file) into the packaged build so installed
# copies run licensed - the key never enters the public repo, only the shipped binary. Without
# the file the installer still builds, but installed copies run in Syncfusion's trial mode.
$licenseFile = Join-Path $repoRoot "syncfusion-license.local.txt"
$licenseArgs = @()
if (Test-Path $licenseFile) {
    Write-Host "Embedding Syncfusion license key from syncfusion-license.local.txt."
    $licenseArgs = @("-p:SyncfusionLicenseFile=$licenseFile")
} else {
    Write-Warning "syncfusion-license.local.txt nicht gefunden - installierte Kopien laufen im Syncfusion-Trial-Modus."
}

dotnet publish $csproj -c Release -r win-x64 --self-contained true -o $publishDir `
    -p:PublishSingleFile=false @licenseArgs

if ($LASTEXITCODE -ne 0) {
    throw "dotnet publish failed with exit code $LASTEXITCODE."
}

Write-Host "Compiling installer with Inno Setup..."

& $iscc "/DAppVersion=$version" "/DSourceDir=$publishDir" $issFile

if ($LASTEXITCODE -ne 0) {
    throw "ISCC.exe failed with exit code $LASTEXITCODE."
}

Write-Host "Installer built: $repoRoot\dist\GMHelper-Setup-$version.exe"
