@echo off
chcp 65001 >nul
setlocal

set "CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

echo Advanced Disk Analyzer v2.0 - Derleniyor...

"%CSC%" /target:winexe /codepage:65001 /win32icon:app.ico /out:DiskAnalyzer.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Management.dll Program.cs
if errorlevel 1 (
    echo.
    echo HATA: DiskAnalyzer.exe derleme başarısız.
    exit /b 1
)

"%CSC%" /target:exe /codepage:65001 /out:LicenseServer.exe /reference:System.Data.dll LicenseServer.cs
if errorlevel 1 (
    echo.
    echo HATA: LicenseServer.exe derleme başarısız.
    exit /b 1
)

"%CSC%" /target:winexe /codepage:65001 /win32icon:app.ico /out:AdvancedDiskAnalyzerSetup.exe /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /resource:DiskAnalyzer.exe,DiskAnalyzer.exe /resource:LicenseServer.exe,LicenseServer.exe Setup.cs
if errorlevel 1 (
    echo.
    echo HATA: AdvancedDiskAnalyzerSetup.exe derleme başarısız.
    exit /b 1
)

echo.
echo Başarılı! DiskAnalyzer.exe, LicenseServer.exe ve AdvancedDiskAnalyzerSetup.exe oluşturuldu.
exit /b 0
