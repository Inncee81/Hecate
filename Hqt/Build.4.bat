@echo off
goto code
*************************************************************************************
* Copyright (C) 2017 Schroedinger Entertainment
* Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)
**************************************************************************************
:code

set "dotNet=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\"
if not exist %dotNet%nul goto error

echo Building for .NET4
if exist "Build.rsp" goto rsp
call Build.Rsp.bat
if %ERRORLEVEL% neq 0 goto end

echo -define:net45;NET45;NET_4_5;NET_FRAMEWORK  >> "Build.rsp"
echo -reference:System.Net.Http.dll             >> "Build.rsp"

:rsp
"%dotNet%csc.exe" @"Build.rsp"
goto end

:error
echo Build requires C# .NET4
set errorlevel=1

:end