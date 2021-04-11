@echo off
goto code
*************************************************************************************
* Copyright (C) 2017 Schroedinger Entertainment
* Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)
**************************************************************************************
:code

setlocal
set errorlevel=0
cd /d %~dp0

echo %~1| findstr /i "[-/]*net5" >nul 2>nul && (set net5=1) || (set net5=0)
if %net5%==1 (call "Build.5.bat") else (call "Build.4.bat")

if %ERRORLEVEL% neq 0 goto end

:end