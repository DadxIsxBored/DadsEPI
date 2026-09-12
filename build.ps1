param(
    [switch]$Package
)

$ErrorActionPreference = 'Stop'
$projectRoot = $PSScriptRoot
$configuration = 'Release'

dotnet build (Join-Path $projectRoot 'DadsEPI.csproj') -c $configuration

if ($Package) {
    $manifestPath = Join-Path $projectRoot 'package\manifest.json'
    $readmePath = Join-Path $projectRoot 'package\README.md'
    $iconPath = Join-Path $projectRoot 'package\icon.png'
    $dllPath = Join-Path $projectRoot 'bin\Release\net48\DadsEPI.dll'
    $manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json

    if ($manifest.name -notmatch '^[A-Za-z0-9_]{1,128}$') {
        throw 'Thunderstore manifest name is invalid.'
    }
    if ($manifest.version_number -notmatch '^\d+\.\d+\.\d+$') {
        throw 'Thunderstore version_number must use Major.Minor.Patch.'
    }
    if ([string]::IsNullOrWhiteSpace($manifest.description) -or $manifest.description.Length -gt 250) {
        throw 'Thunderstore description must contain 1 through 250 characters.'
    }
    if ($null -eq $manifest.dependencies) {
        throw 'Thunderstore dependencies must be present.'
    }
    foreach ($requiredFile in @($manifestPath, $readmePath, $iconPath, $dllPath, (Join-Path $projectRoot 'CHANGELOG.md'), (Join-Path $projectRoot 'LICENSE'))) {
        if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
            throw "Required package file is missing: $requiredFile"
        }
    }

    Add-Type -AssemblyName System.Drawing
    $icon = [System.Drawing.Image]::FromFile($iconPath)
    try {
        if ($icon.Width -ne 256 -or $icon.Height -ne 256 -or $icon.RawFormat.Guid -ne [System.Drawing.Imaging.ImageFormat]::Png.Guid) {
            throw 'Thunderstore icon.png must be a 256x256 PNG.'
        }
    }
    finally {
        $icon.Dispose()
    }

    $assemblyVersion = [Reflection.AssemblyName]::GetAssemblyName($dllPath).Version
    $expectedVersion = [Version]$manifest.version_number
    if ($assemblyVersion.Major -ne $expectedVersion.Major -or $assemblyVersion.Minor -ne $expectedVersion.Minor -or $assemblyVersion.Build -ne $expectedVersion.Build) {
        throw "DLL version $assemblyVersion does not match manifest version $($manifest.version_number)."
    }

    $distRoot = Join-Path $projectRoot 'dist'
    $archiveRoot = Join-Path $projectRoot 'Archive\package-builds'
    New-Item -ItemType Directory -Path $distRoot -Force | Out-Null
    New-Item -ItemType Directory -Path $archiveRoot -Force | Out-Null
    $stamp = Get-Date -Format 'yyyyMMdd-HHmmss-fff'
    foreach ($artifact in Get-ChildItem -LiteralPath $distRoot -Force) {
        $archiveName = if ($artifact.PSIsContainer) {
            "$($artifact.Name)-$stamp"
        }
        else {
            "$($artifact.BaseName)-$stamp$($artifact.Extension)"
        }
        Move-Item -LiteralPath $artifact.FullName -Destination (Join-Path $archiveRoot $archiveName)
    }

    $dist = Join-Path $distRoot "DadsEPI-$($manifest.version_number)"
    $zipPath = Join-Path $distRoot "DadsEPI-$($manifest.version_number).zip"
    New-Item -ItemType Directory -Path $dist | Out-Null
    Copy-Item -LiteralPath $dllPath -Destination $dist
    Copy-Item -LiteralPath $manifestPath -Destination $dist
    Copy-Item -LiteralPath $readmePath -Destination $dist
    Copy-Item -LiteralPath $iconPath -Destination $dist
    Copy-Item -LiteralPath (Join-Path $projectRoot 'CHANGELOG.md') -Destination $dist
    Copy-Item -LiteralPath (Join-Path $projectRoot 'LICENSE') -Destination $dist

    Compress-Archive -LiteralPath (Get-ChildItem -LiteralPath $dist -File).FullName -DestinationPath $zipPath -CompressionLevel Optimal

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
    try {
        $entryNames = @($archive.Entries | ForEach-Object { $_.FullName })
        foreach ($requiredEntry in @('manifest.json', 'README.md', 'icon.png', 'DadsEPI.dll')) {
            if ($entryNames -notcontains $requiredEntry) {
                throw "ZIP root is missing $requiredEntry."
            }
        }
        if ($entryNames | Where-Object { $_ -match '(^|/)(assembly_valheim|UnityEngine|BepInEx|0Harmony)\.dll$' }) {
            throw 'ZIP contains a prohibited game, loader, or framework assembly.'
        }
    }
    finally {
        $archive.Dispose()
    }

    Write-Output "Thunderstore package: $zipPath"
    Write-Output "SHA256: $((Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash)"
}
