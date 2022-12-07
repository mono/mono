@echo off
rem This runs genmdesc on the x86 files when called on Visual Studio
echo Running genmdesc
cd ..\mono\mini
set PATH=%PATH%;%MONO_DEPENDENCIES_PREFIX%\bin
if "%2" == "Win32" goto x86
if "%3" == "Release-target-arm64" goto arm64
if "%3" == "Debug-target-arm64" goto arm64
if "%2" == "x64" goto x64
goto error
:x86
echo Platform detected is x86...
%1 cpu-x86.h x86_desc cpu-x86.md
goto end
:x64
echo Platform detected is x64...
%1 cpu-amd64.h amd64_desc cpu-amd64.md
goto end
:arm64
echo Platform detected is ARM64...
%1 cpu-arm64.h arm64_cpu_desc cpu-arm64.md
goto end
:error
echo Error: unsupported platform
exit /b 100
:end
echo done

