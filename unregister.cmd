@echo off
SET PASSIEDLL=Release\PassIE.dll
IF NOT [%1]==[] SET PASSIEDLL=%1\PassIE.dll

C:\Windows\Microsoft.NET\Framework64\v4.0.30319\RegAsm.exe /unregister bin\x64\%PASSIEDLL%
C:\Windows\Microsoft.NET\Framework\v4.0.30319\RegAsm.exe /unregister bin\x86\%PASSIEDLL%