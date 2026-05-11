@echo off
echo Advanced Disk Analyzer v1.9 Beta - Derleniyor...

"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ^
  /target:winexe ^
  /win32icon:app.ico ^
  /out:DiskAnalyzer.exe ^
  /reference:System.Windows.Forms.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Management.dll ^
  Program.cs

if not %errorlevel% == 0 (
    echo.
    echo HATA: DiskAnalyzer.exe derleme basarisiz.
    pause
    exit /b 1
)

"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ^
  /target:exe ^
  /out:LicenseServer.exe ^
  /reference:System.Data.dll ^
  LicenseServer.cs

if not %errorlevel% == 0 (
    echo.
    echo HATA: LicenseServer.exe derleme basarisiz.
    pause
    exit /b 1
)

"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ^
  /target:winexe ^
  /win32icon:app.ico ^
  /out:AdvancedDiskAnalyzerSetup.exe ^
  /reference:System.Windows.Forms.dll ^
  /reference:System.Drawing.dll ^
  /resource:DiskAnalyzer.exe,DiskAnalyzer.exe ^
  /resource:LicenseServer.exe,LicenseServer.exe ^
  Setup.cs

if not %errorlevel% == 0 (
    echo.
    echo HATA: AdvancedDiskAnalyzerSetup.exe derleme basarisiz.
    pause
    exit /b 1
)

echo.
echo Basarili! DiskAnalyzer.exe, LicenseServer.exe ve AdvancedDiskAnalyzerSetup.exe olusturuldu.
pause
