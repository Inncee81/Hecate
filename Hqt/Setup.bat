@echo off
goto code
*************************************************************************************
* Copyright (C) 2017 Schroedinger Entertainment
* Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)
**************************************************************************************
:code

setlocal
cd /d %~dp0

if exist "Build.rsp" (del "Build.rsp")
call Build.bat %*