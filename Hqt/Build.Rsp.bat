@echo off
goto code
*************************************************************************************
* Copyright (C) 2017 Schroedinger Entertainment
* Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)
**************************************************************************************
:code

break                                          > "Build.rsp"
echo -nologo                                  >> "Build.rsp"
echo -optimize+                               >> "Build.rsp"
echo -nowarn:0436,1685                        >> "Build.rsp"
echo -out:"..\..\Hqt.exe"                     >> "Build.rsp"
echo -platform:x64                            >> "Build.rsp"
echo -target:exe                              >> "Build.rsp"

if not exist "..\..\Sharp\Actor" goto actor_package
echo -recurse:"..\..\Sharp\Actor\*.cs"        >> "Build.rsp"
goto actor_end

:actor_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.actor@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                 >> "Build.rsp"

:actor_end

if not exist "..\..\Sharp\Alchemy" goto alchemy_package
echo -recurse:"..\..\Sharp\Alchemy\*.cs"       >> "Build.rsp"
goto alchemy_end

:alchemy_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.alchemy@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                 >> "Build.rsp"

:alchemy_end

if not exist "..\..\Sharp\App" goto app_package
echo -recurse:"..\..\Sharp\App\*.cs"           >> "Build.rsp"
goto app_end

:app_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.app@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:app_end

if not exist "..\..\Sharp\CommandLine" goto cmd_package
echo -recurse:"..\..\Sharp\CommandLine\*.cs"  >> "Build.rsp"
goto cmd_end

:cmd_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.command-line@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:cmd_end

if not exist "..\..\Sharp\Common" goto com_package
echo -recurse:"..\..\Sharp\Common\*.cs"       >> "Build.rsp"
goto com_end

:com_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.common@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:com_end

if not exist "..\..\Sharp\Config" goto conf_package
echo -recurse:"..\..\Sharp\Config\*.cs"       >> "Build.rsp"
goto conf_end

:conf_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.config@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:conf_end

if not exist "..\..\Sharp\Flex" goto flex_package
echo -recurse:"..\..\Sharp\Flex\*.cs"         >> "Build.rsp"
goto flex_end

:flex_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.flex@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:flex_end

if not exist "..\..\Sharp\Json" goto json_package
echo -recurse:"..\..\Sharp\Json\*.cs"         >> "Build.rsp"
goto json_end

:json_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.json@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:json_end

if not exist "..\..\Sharp\Parsing" goto parse_package
echo -recurse:"..\..\Sharp\Parsing\*.cs"      >> "Build.rsp"
goto parse_end

:parse_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.parsing@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:parse_end

if not exist "..\..\Sharp\Reactive" goto react_package
echo -recurse:"..\..\Sharp\Reactive\*.cs"     >> "Build.rsp"
goto react_end

:react_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.reactive@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:react_end

if not exist "..\..\Sharp\SharpLang" goto sharp_package
echo -recurse:"..\..\Sharp\SharpLang\*.cs"    >> "Build.rsp"
goto sharp_end

:sharp_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.sharp-lang@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:sharp_end

if not exist "..\..\Sharp\Tar" goto tar_package
echo -recurse:"..\..\Sharp\Tar\*.cs"          >> "Build.rsp"
goto tar_end

:tar_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.tar@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:tar_end

if not exist "..\..\Sharp\Web" goto web_package
echo -recurse:"..\..\Sharp\Web\*.cs"          >> "Build.rsp"
goto web_end

:web_package
set "package=..\..\Packages"
for /f "delims=" %%a in ('dir /b "%package%\se.sharp.web@*"') do set "ver=%%a"
set "package=%package%\%ver%"

if not exist "%package%" goto error
echo -recurse:"%package%\*.cs"                >> "Build.rsp"

:web_end

echo -recurse:"..\..\Apollo\Package\*.cs"     >> "Build.rsp"
echo -recurse:"*.cs"                          >> "Build.rsp"
goto end

:error
echo Missing dependency '%package%'
set errorlevel=1

:end

