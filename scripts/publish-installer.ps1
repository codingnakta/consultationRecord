param(
    [string]$Version = "1.0.0",
    [string]$Runtime = "win-x64",
    [switch]$SkipInstaller
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$project = Join-Path $root "StudentCounseling\StudentCounseling.csproj"
$publishDir = Join-Path $root "artifacts\publish"
$installerOut = Join-Path $root "artifacts\installer"

Remove-Item -LiteralPath $publishDir -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $installerOut -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Path $publishDir, $installerOut | Out-Null

dotnet publish $project `
    -c Release `
    -r $Runtime `
    --self-contained true `
    -p:PublishSingleFile=false `
    -p:Version=$Version `
    -o $publishDir

if ($SkipInstaller) {
    Write-Host "Published app to $publishDir"
    exit 0
}

$isccCommand = Get-Command ISCC.exe -ErrorAction SilentlyContinue
$isccPath = if ($isccCommand) { $isccCommand.Source } else { $null }
if (-not $isccPath) {
    $candidate = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
    if (Test-Path $candidate) {
        $isccPath = $candidate
    }
}

if (-not $isccPath) {
    throw "Inno Setup 6 ISCC.exe를 찾지 못했습니다. 설치 후 다시 실행하거나 -SkipInstaller로 publish만 수행하세요."
}

$env:APP_VERSION = $Version
$env:PUBLISH_DIR = $publishDir
$env:INSTALLER_OUTPUT_DIR = $installerOut

& $isccPath (Join-Path $root "installer\StudentCounseling.iss")

Write-Host "Installer output:"
Get-ChildItem $installerOut
