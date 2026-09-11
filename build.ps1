param(
    [switch]$Package
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$configuration = 'Release'

dotnet build (Join-Path $projectRoot 'DadsEPI.csproj') -c $configuration

if ($Package) {
    $manifest = Get-Content -LiteralPath (Join-Path $projectRoot 'package\manifest.json') -Raw | ConvertFrom-Json
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss'
    $dist = Join-Path $projectRoot "dist\DadsEPI-$($manifest.version_number)-$stamp"
    if (Test-Path -LiteralPath $dist) {
        throw "Package staging directory already exists: $dist"
    }
    New-Item -ItemType Directory -Path $dist | Out-Null
    Copy-Item -LiteralPath (Join-Path $projectRoot 'bin\Release\net48\DadsEPI.dll') -Destination $dist
    Copy-Item -LiteralPath (Join-Path $projectRoot 'package\manifest.json') -Destination $dist
    Copy-Item -LiteralPath (Join-Path $projectRoot 'package\README.md') -Destination $dist
    Copy-Item -LiteralPath (Join-Path $projectRoot 'CHANGELOG.md') -Destination $dist
    Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination $dist
}
