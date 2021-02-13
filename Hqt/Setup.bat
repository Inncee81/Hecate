@ECHO off
REM Copyright (C) 2017 Schroedinger Entertainment
REM Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

set "dotNet=%SystemRoot%\Microsoft.NET\Framework64\v4.0.30319\"
if not exist %dotNet%nul goto WIN_ERROR

setlocal
cd /d %~dp0

"%dotNet%csc.exe" @"Setup.inc"
if %ERRORLEVEL% neq 0 goto EXIT
goto EXIT

:WIN_ERROR
echo Build requires C# .Net 4.0 or higher

:EXIT