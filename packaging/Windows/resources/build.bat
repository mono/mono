@echo off

set MONO_VERSION=X.X.X
set GTKSHARP_VERSION=X.X.X
set MONO_FILES_DIR=..\tmp\mono

"%windir%\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe" %~dp0\MonoForWindows.wixproj /p:Configuration=Release /p:DefaultCompressionLevel=high
