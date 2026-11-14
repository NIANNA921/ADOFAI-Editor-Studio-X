param(
    [string]$GameDir = "E:\SteamLibrary\steamapps\common\A Dance of Fire and Ice",
    [switch]$SkipPackage
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "ADOFAIEditorStudio.csproj"
$infoPath = Join-Path $PSScriptRoot "Info.json"
$managed = Join-Path $GameDir "A Dance of Fire and Ice_Data\Managed"

Write-Host "Checking game files..."
if (-not (Test-Path $project)) {
    Write-Host "Project file not found." -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $infoPath)) {
    Write-Host "Info.json not found." -ForegroundColor Red
    exit 1
}
if (-not (Test-Path $managed)) {
    Write-Host "Game Managed folder not found:" -ForegroundColor Red
    Write-Host $managed
    exit 1
}

# ── 版本必须只有一处真相 ───────────────────────────────────────────
# Info.json 是 UMM 读的版本，csproj 是程序集版本。两边不一致时直接失败，
# 避免出现「UMM 显示 0.95.0、DLL 里还是 0.39.5」这种问题。
$infoText = (Get-Content -LiteralPath $infoPath) -join [Environment]::NewLine
$info = $infoText | ConvertFrom-Json
$version = [string]$info.Version
$displayName = [string]$info.DisplayName
$assemblyName = [string]$info.AssemblyName
if ([string]::IsNullOrWhiteSpace($version)) {
    Write-Host ("Info.json has no Version. Parsed: " + ($info | Out-String)) -ForegroundColor Red
    exit 1
}
if ([string]::IsNullOrWhiteSpace($assemblyName)) {
    Write-Host "Info.json has no AssemblyName." -ForegroundColor Red
    exit 1
}
$assemblyFile = $assemblyName
if (-not $assemblyFile.EndsWith(".dll", [System.StringComparison]::OrdinalIgnoreCase)) {
    $assemblyFile += ".dll"
}

[xml]$projectXml = Get-Content $project -Raw
$projectVersion = [string]$projectXml.Project.PropertyGroup.Version
# Info.json 允许 A/B/C 微调后缀（例如 0.95.0A），程序集版本只能是数字核。
# 两边允许「完整版本 vs 数字核」，需要时把 csproj 规范化后再交给 dotnet。
$versionCore = $version -replace '[A-Za-z]+$', ''
if ($projectVersion -ne $version -and $projectVersion -ne $versionCore) {
    Write-Host ("Version mismatch: Info.json = " + $version + ", csproj = " + $projectVersion) -ForegroundColor Red
    Write-Host "Change both to the same numeric core before building." -ForegroundColor Red
    exit 1
}
if ($projectVersion -ne $versionCore) {
    $projectText = [IO.File]::ReadAllText($project)
    $projectText = [regex]::Replace($projectText, '<Version>.*?</Version>', "<Version>$versionCore</Version>")
    [IO.File]::WriteAllText($project, $projectText, (New-Object System.Text.UTF8Encoding($true)))
    $projectVersion = $versionCore
}

$required = @(
    "Assembly-CSharp.dll",
    "Assembly-CSharp-firstpass.dll",
    "DOTween.dll",
    "RDTools.dll",
    "UnityEngine.dll",
    "UnityEngine.CoreModule.dll",
    "UnityEngine.AudioModule.dll",
    "UnityEngine.TextRenderingModule.dll",
    "UnityEngine.SpriteMaskModule.dll",
    "UnityEngine.InputLegacyModule.dll",
    "Newtonsoft.Json.dll",
    "UnityEngine.Physics2DModule.dll",
    "UnityEngine.IMGUIModule.dll",
    "UnityEngine.ParticleSystemModule.dll",
    "UnityEngine.UI.dll",
    "UnityEngine.UIModule.dll",
    "UnityEngine.TextCoreFontEngineModule.dll",
    "UnityEngine.TextCoreTextEngineModule.dll",
    "Unity.TextMeshPro.dll",
    "UnityEngine.AssetBundleModule.dll"
)

$missing = @()
foreach ($file in $required) {
    if (-not (Test-Path (Join-Path $managed $file))) {
        $missing += $file
    }
}

$umm = Join-Path $managed "UnityModManager"
foreach ($file in @("0Harmony.dll", "UnityModManager.dll")) {
    if (-not (Test-Path (Join-Path $umm $file))) {
        $missing += ("UnityModManager\" + $file)
    }
}

if ($missing.Count -gt 0) {
    Write-Host "Missing game or Unity Mod Manager files:" -ForegroundColor Red
    foreach ($file in $missing) {
        Write-Host (" - " + $file)
    }
    Write-Host "Check the game path and make sure Unity Mod Manager is installed."
    exit 1
}

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host ".NET SDK was not found. Install the .NET SDK first." -ForegroundColor Red
    exit 1
}

Write-Host ("Building " + $displayName + " v" + $version) -ForegroundColor Cyan
Write-Host ("Game folder: " + $GameDir)
dotnet build $project -c Release "-p:GameDir=$GameDir"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed." -ForegroundColor Red
    exit $LASTEXITCODE
}

$builtDll = Join-Path $PSScriptRoot ("bin\Release\" + $assemblyFile)
if (-not (Test-Path $builtDll)) {
    Write-Host ("Built DLL not found: " + $builtDll) -ForegroundColor Red
    exit 1
}

if (-not $SkipPackage) {
    # 输出目录名 = UMM 的 mod 文件夹名，不是程序集名。
    $out = Join-Path $PSScriptRoot "dist\ADOFAIEditorStudioX"
    if (Test-Path $out) { Remove-Item $out -Recurse -Force }
    New-Item -ItemType Directory -Force -Path $out | Out-Null

    Copy-Item $infoPath $out -Force
    Copy-Item (Join-Path $PSScriptRoot "AesIcon.rgba") $out -Force
    Copy-Item (Join-Path $PSScriptRoot "LICENSE") $out -Force
    Copy-Item $builtDll $out -Force
    $font = Join-Path $PSScriptRoot "AesxFont.ttf"
    if (Test-Path $font)
    {
        Copy-Item $font $out -Force
    }
    else
    {
        Write-Host "Warning: AesxFont.ttf not found; Paper theme will fall back to system font." -ForegroundColor Yellow
    }

    $lightLogo = Join-Path $PSScriptRoot "AesxLogoLight.png"
    if (Test-Path $lightLogo)
    {
        Copy-Item $lightLogo $out -Force
    }

    $logo = Join-Path $PSScriptRoot "AesxLogo.png"
    if (Test-Path $logo)
    {
        Copy-Item $logo $out -Force
    }
    else
    {
        Write-Host "Warning: AesxLogo.png not found; version page will not show the logo." -ForegroundColor Yellow
    }

    $zip = Join-Path $PSScriptRoot ("dist\ADOFAIEditorStudioX-v" + $version + ".zip")
    if (Test-Path $zip) { Remove-Item $zip -Force }
    # 把文件夹本身压进去，解压到 Mods 下就是 Mods\ADOFAIEditorStudioX\...
    Compress-Archive -Path $out -DestinationPath $zip -Force

    Write-Host ("Mod package created: " + $zip) -ForegroundColor Green
    Write-Host "Extract the zip into the game's Mods folder (it creates Mods\ADOFAIEditorStudioX)."
    Write-Host "If an old ADOFAIEditorStudio.dll remains in that folder, delete it first."
}
