@echo off
goto code
*************************************************************************************
* Copyright (C) 2017 Schroedinger Entertainment
* Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)
**************************************************************************************
:code

set "dotNet=%ProgramFiles%\dotnet\sdk"
for /f "delims=" %%a in ('dir /b "%dotNet%\5.*"') do set "ver=%%a"
set "dotNet=%dotNet%\%ver%\Roslyn\bincore"

if not exist "%dotNet%" goto error

set "dotNetCore=%ProgramFiles%\dotnet\shared\Microsoft.NETCore.App"
for /f "delims=" %%a in ('dir /b "%dotNetCore%\5.*"') do set "ver=%%a"
set "dotNetCore=%dotNetCore%\%ver%"

if not exist "%dotNetCore%" goto error

set "dotNetWin=%ProgramFiles%\dotnet\shared\Microsoft.WindowsDesktop.App"
for /f "delims=" %%a in ('dir /b "%dotNetWin%\5.*"') do set "ver=%%a"
set "dotNetWin=%dotNetWin%\%ver%"

if not exist "%dotNetWin%" goto error

echo Building for .NET Core 5
if exist "Build.rsp" goto rsp
call Build.Rsp.bat
if %ERRORLEVEL% neq 0 goto end

echo -define:net50;NET50;NET_5_0;NET_CORE                                         >> "Build.rsp"
echo -reference:"%dotNetCore%\netstandard.dll"                                    >> "Build.rsp"
echo -reference:"%dotNetCore%\mscorlib.dll"                                       >> "Build.rsp"
echo -reference:"%dotNetCore%\System.dll"                                         >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Collections.dll"                             >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Collections.Concurrent.dll"                  >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Collections.Specialized.dll"                 >> "Build.rsp"
echo -reference:"%dotNetCore%\System.ComponentModel.dll"                          >> "Build.rsp"
echo -reference:"%dotNetCore%\System.ComponentModel.Primitives.dll"               >> "Build.rsp"
echo -reference:"%dotNetCore%\System.ComponentModel.TypeConverter.dll"            >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Console.dll"                                 >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Core.dll"                                    >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Diagnostics.Process.dll"                     >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Diagnostics.FileVersionInfo.dll"             >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Dynamic.Runtime.dll"                         >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Globalization.dll"                           >> "Build.rsp"
echo -reference:"%dotNetCore%\System.IO.dll"                                      >> "Build.rsp"
echo -reference:"%dotNetCore%\System.IO.Compression.dll"                          >> "Build.rsp"
echo -reference:"%dotNetCore%\System.IO.FileSystem.dll"                           >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Linq.dll"                                    >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Linq.Expressions.dll"                        >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.dll"                                     >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.Http.dll"                                >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.NameResolution.dll"                      >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.NetworkInformation.dll"                  >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.Primitives.dll"                          >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.Security.dll"                            >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.ServicePoint.dll"                        >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.Sockets.dll"                             >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Net.WebClient.dll"                           >> "Build.rsp"
echo -reference:"%dotNetCore%\System.ObjectModel.dll"                             >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Private.CoreLib.dll"                         >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Private.Uri.dll"                             >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Reflection.dll"                              >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Reflection.Primitives.dll"                   >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Reflection.Emit.dll"                         >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Reflection.Emit.ILGeneration.dll"            >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Resources.Writer.dll"                        >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Runtime.dll"                                 >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Runtime.InteropServices.dll"                 >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Runtime.Loader.dll"                          >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Security.dll"                                >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Security.Cryptography.Algorithms.dll"        >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Security.Cryptography.Primitives.dll"        >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Security.Cryptography.X509Certificates.dll"  >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Threading.dll"                               >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Threading.Tasks.dll"                         >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Threading.Thread.dll"                        >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Threading.Tasks.Parallel.dll"                >> "Build.rsp"
echo -reference:"%dotNetCore%\System.ValueTuple.dll"                              >> "Build.rsp"
echo -reference:"%dotNetCore%\System.Windows.dll"                                 >> "Build.rsp"
echo -reference:"%dotNetCore%\WindowsBase.dll"                                    >> "Build.rsp"
echo -reference:"%dotNetWin%\System.Windows.Forms.dll"                            >> "Build.rsp"
echo -reference:"%dotNetWin%\System.Windows.Extensions.dll"                       >> "Build.rsp"

:rsp

dotnet "%dotNet%\csc.dll" @"Build.rsp"
if %ERRORLEVEL% neq 0 goto end

echo {"runtimeOptions":{"tfm":"net5.0","frameworks":[{"name":"Microsoft.NETCore.App","version":"%ver%"},{"name":"Microsoft.WindowsDesktop.App","version":"%ver%"}]}} > ..\..\Hqt.runtimeconfig.json
goto end

:error
echo Build requires C# .NET Core 5
set errorlevel=1

:end