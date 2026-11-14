[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Version,

    [string]$OutputDirectory,
    [string]$MoonSweepersGameRoot,
    [string]$DotnetPath
)

$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $projectRoot 'WhoLeftTheLightOn.csproj'
$buildOutput = Join-Path $projectRoot 'bin\Release\net6.0'

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) { $OutputDirectory = Join-Path $projectRoot 'dist' }
if ([string]::IsNullOrWhiteSpace($MoonSweepersGameRoot)) { $MoonSweepersGameRoot = 'C:\Program Files (x86)\Steam\steamapps\common\Moon Sweepers' }
if ([string]::IsNullOrWhiteSpace($DotnetPath)) {
    $legacyDotnet = Join-Path $projectRoot '..\Quantum Conveyors\work\dotnet-sdk\dotnet.exe'
    $DotnetPath = if (Test-Path -LiteralPath $legacyDotnet) { $legacyDotnet } else { 'dotnet' }
}

if ($Version -notmatch '^\d+(\.\d+){1,2}$') { throw "Version '$Version' is not a simple dotted version such as 0.1.0." }
if (-not (Test-Path -LiteralPath $project)) { throw "Plugin project was not found: $project" }
if (-not (Test-Path -LiteralPath $MoonSweepersGameRoot)) { throw "Moon Sweepers was not found: $MoonSweepersGameRoot" }

$pluginSource = Get-Content -LiteralPath (Join-Path $projectRoot 'Plugin.cs') -Raw
if ($pluginSource -notmatch ('PluginVersion\s*=\s*"' + [regex]::Escape($Version) + '"')) {
    throw "Plugin.cs does not declare version $Version. Update PluginVersion before publishing this release."
}

& $DotnetPath build $project -c Release -p:MoonSweepersGameRoot=$MoonSweepersGameRoot
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
if (-not (Test-Path -LiteralPath (Join-Path $buildOutput 'WhoLeftTheLightOn.dll'))) { throw 'Build completed without producing WhoLeftTheLightOn.dll.' }

$packageRoot = Join-Path $OutputDirectory "WhoLeftTheLightOn-$Version"
$pluginDestination = Join-Path $packageRoot 'BepInEx\plugins\WhoLeftTheLightOn'
$archivePath = Join-Path $OutputDirectory "WhoLeftTheLightOn-$Version.zip"

if (Test-Path -LiteralPath $packageRoot) { Remove-Item -LiteralPath $packageRoot -Recurse -Force }
if (Test-Path -LiteralPath $archivePath) { Remove-Item -LiteralPath $archivePath -Force }
New-Item -ItemType Directory -Force -Path $pluginDestination | Out-Null
Copy-Item -LiteralPath (Join-Path $buildOutput 'WhoLeftTheLightOn.dll') -Destination $pluginDestination -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'README.md') -Destination (Join-Path $packageRoot 'README.txt') -Force
Copy-Item -LiteralPath (Join-Path $projectRoot 'CHANGELOG.md') -Destination (Join-Path $packageRoot 'CHANGELOG.txt') -Force
Compress-Archive -Path (Join-Path $packageRoot '*') -DestinationPath $archivePath -CompressionLevel Optimal
Write-Host "Created $archivePath"

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_OUTPUT)) { Add-Content -LiteralPath $env:GITHUB_OUTPUT -Value "package=$archivePath" }
