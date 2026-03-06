@echo off
echo Advanced Disk Analyzer v1.6 - Derleniyor...

"C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe" ^
  /target:winexe ^
  /out:DiskAnalyzer.exe ^
  /reference:System.Windows.Forms.dll ^
  /reference:System.Drawing.dll ^
  /reference:System.Management.dll ^
  Program.cs

if %errorlevel% == 0 (
    echo.
    echo Basarili! DiskAnalyzer.exe olusturuldu.
) else (
    echo.
    echo HATA: Derleme basarisiz.
)
pause
