param(
	[string]$Configuration = "Release",
	[string]$ProjectPath = "$PSScriptRoot\Orbitfall.Mod.csproj",
	[string]$StagingPath = "$PSScriptRoot\..\..\artifacts\mod\Orbitfall",
	[string]$GameLocalModPath = "$(Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::MyDocuments)) 'Klei\OxygenNotIncluded\mods\Local\Orbitfall')",
	[switch]$InstallToGame
)

$ErrorActionPreference = "Stop"

if (Test-Path $StagingPath) {
	Get-ChildItem -Path $StagingPath -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
}

dotnet build $ProjectPath -c $Configuration

if ($InstallToGame) {
	New-Item -ItemType Directory -Force -Path $GameLocalModPath | Out-Null
	Get-ChildItem -Path $GameLocalModPath -Force -ErrorAction SilentlyContinue | Remove-Item -Recurse -Force
	Copy-Item -Path (Join-Path $StagingPath '*') -Destination $GameLocalModPath -Recurse -Force
	Write-Host "Installed Orbitfall mod to $GameLocalModPath"
}
