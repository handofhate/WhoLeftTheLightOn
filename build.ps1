param(
    [string]$MoonSweepersGameRoot,
    [string]$DotnetPath,
    [switch]$Install
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$game = if ([string]::IsNullOrWhiteSpace($MoonSweepersGameRoot)) { 'C:\Program Files (x86)\Steam\steamapps\common\Moon Sweepers' } else { $MoonSweepersGameRoot }
$legacyDotnet = Join-Path $root '..\Quantum Conveyors\work\dotnet-sdk\dotnet.exe'
if ([string]::IsNullOrWhiteSpace($DotnetPath)) {
    if (Test-Path -LiteralPath $legacyDotnet) { $DotnetPath = $legacyDotnet } else { $DotnetPath = 'dotnet' }
}

$project = Join-Path $root 'WhoLeftTheLightOn.csproj'
& $DotnetPath build $project -c Release -p:MoonSweepersGameRoot=$game
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($Install) {
    $destination = Join-Path $game 'BepInEx\plugins\WhoLeftTheLightOn'
    New-Item -ItemType Directory -Force -Path $destination | Out-Null
    Copy-Item -LiteralPath (Join-Path $root 'bin\Release\net6.0\WhoLeftTheLightOn.dll') -Destination $destination -Force
    Write-Host "Installed to $destination"
}
