@echo off
chcp 65001 >nul
title ADOFAI Editor Studio X - Build Mod
echo ========================================
echo   ADOFAI Editor Studio X - Build
echo ========================================
echo.
echo Checking and building the mod, please wait...
echo If the .NET SDK or game files are missing, send the error text back.
echo.
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-mod.ps1"
echo.
if errorlevel 1 (
  echo [FAILED] Build did not finish. Please send the error above.
) else (
  echo [OK] Mod package is in the dist folder.
)
echo.
pause
