@ECHO off
REM Copyright (C) 2017 Schroedinger Entertainment
REM Distributed under the Schroedinger Entertainment EULA (See EULA.md for details)

set "dotNet=%ProgramFiles%\dotnet\sdk"
for /f "delims=" %%a in ('dir /b "%dotNet%\5.*"') do set "ver=%%a"
set "dotNet=%dotNet%\%ver%\Roslyn\bincore"

set "dotNetCore=%ProgramFiles%\dotnet\shared\Microsoft.NETCore.App"
for /f "delims=" %%a in ('dir /b "%dotNetCore%\5.*"') do set "ver=%%a"
set "dotNetCore=%dotNetCore%\%ver%"

set "dotNetWin=%ProgramFiles%\dotnet\shared\Microsoft.WindowsDesktop.App"
for /f "delims=" %%a in ('dir /b "%dotNetWin%\5.*"') do set "ver=%%a"
set "dotNetWin=%dotNetWin%\%ver%"

if not exist "%dotNet%" goto WIN_ERROR
if not exist "%dotNetCore%" goto WIN_ERROR
if not exist "%dotNetWin%" goto WIN_ERROR

break                                                                              > "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\netstandard.dll"                                    >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\mscorlib.dll"                                       >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.dll"                                         >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Collections.dll"                             >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Collections.Concurrent.dll"                  >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Collections.Specialized.dll"                 >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.ComponentModel.dll"                          >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.ComponentModel.Primitives.dll"               >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.ComponentModel.TypeConverter.dll"            >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Console.dll"                                 >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Core.dll"                                    >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Diagnostics.Process.dll"                     >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Diagnostics.FileVersionInfo.dll"             >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Dynamic.Runtime.dll"                         >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Globalization.dll"                           >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.IO.dll"                                      >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.IO.Compression.dll"                          >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.IO.FileSystem.dll"                           >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Linq.dll"                                    >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Linq.Expressions.dll"                        >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.dll"                                     >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.Http.dll"                                >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.NameResolution.dll"                      >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.NetworkInformation.dll"                  >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.Primitives.dll"                          >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.Security.dll"                            >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.ServicePoint.dll"                        >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.Sockets.dll"                             >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Net.WebClient.dll"                           >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.ObjectModel.dll"                             >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Private.CoreLib.dll"                         >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Private.Uri.dll"                             >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Reflection.dll"                              >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Reflection.Primitives.dll"                   >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Reflection.Emit.dll"                         >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Reflection.Emit.ILGeneration.dll"            >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Resources.Writer.dll"                        >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Runtime.dll"                                 >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Runtime.InteropServices.dll"                 >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Runtime.Loader.dll"                          >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Security.dll"                                >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Security.Cryptography.Algorithms.dll"        >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Security.Cryptography.Primitives.dll"        >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Security.Cryptography.X509Certificates.dll"  >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Threading.dll"                               >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Threading.Tasks.dll"                         >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Threading.Thread.dll"                        >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Threading.Tasks.Parallel.dll"                >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.ValueTuple.dll"                              >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\System.Windows.dll"                                 >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetCore%\WindowsBase.dll"                                    >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetWin%\\System.Windows.Forms.dll"                           >> "%TEMP%\roslyn.rsp"
echo -reference:"%dotNetWin%\\System.Windows.Extensions.dll"                      >> "%TEMP%\roslyn.rsp"

setlocal
cd /d %~dp0

dotnet "%dotNet%\csc.dll" @"%TEMP%\roslyn.rsp" @"Setup.5.inc"
if %ERRORLEVEL% neq 0 goto EXIT

echo {"runtimeOptions":{"tfm":"net5.0","frameworks":[{"name":"Microsoft.NETCore.App","version":"%ver%"},{"name":"Microsoft.WindowsDesktop.App","version":"%ver%"}]}} > ..\..\Hqt.runtimeconfig.json

goto EXIT

:WIN_ERROR
echo Build requires C# .Net 5.0 or higher

:EXIT