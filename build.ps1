$ErrorActionPreference = 'Stop'

$root = $PSScriptRoot
$game = 'C:\Game\Steam\steamapps\common\Slay the Spire 2'
$godot = 'C:\Godot&GDRE\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe'
$modOut = Join-Path $game 'mods\RevertSynchronize'

Write-Host "==> Build .NET assembly"
dotnet build (Join-Path $root 'RevertSynchronize.csproj') -c Debug -p:NuGetAudit=false
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed' }

$dll = Join-Path $root '.godot\mono\temp\bin\Debug\RevertSynchronize.dll'
$pdb = Join-Path $root '.godot\mono\temp\bin\Debug\RevertSynchronize.pdb'
if (-not (Test-Path $dll)) { throw "DLL not found: $dll" }

Write-Host "==> Godot import (generate .import/.ctex metadata)"
Push-Location $root
try {
    & $godot '--headless' '--import' '--path' $root 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Godot import failed: $LASTEXITCODE" }

    Write-Host "==> Godot export pck"
    New-Item -ItemType Directory -Force -Path $modOut | Out-Null
    $pck = Join-Path $modOut 'RevertSynchronize.pck'
    & $godot '--headless' '--export-pack' 'Windows Desktop' $pck '--path' $root 2>&1 | Out-Host
    if ($LASTEXITCODE -ne 0) { throw "Godot export-pack failed: $LASTEXITCODE" }
}
finally {
    Pop-Location
}

Write-Host "==> Copy files to $modOut"
Copy-Item $dll (Join-Path $modOut 'RevertSynchronize.dll') -Force
if (Test-Path $pdb) { Copy-Item $pdb (Join-Path $modOut 'RevertSynchronize.pdb') -Force }
Copy-Item (Join-Path $root 'RevertSynchronize.json') (Join-Path $modOut 'RevertSynchronize.json') -Force

Write-Host '==> Done'
