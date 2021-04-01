:: Set up build environment and build LLVM using MSVC and msbuild targets in mono.sln.

:: Arguments:
:: -------------------------------------------------------
:: %1 Visual Studio target, build|clean, default build
:: %2 Host CPU architecture, x86_64|i686, default x86_64
:: %3 Visual Studio configuration, debug|release, default release
:: %4 Mono MSVC source folder.
:: %5 LLVM build directory.
:: %6 LLVM install directory.
:: %7 Additional arguments passed to msbuild, needs to be quoted if multiple.
:: -------------------------------------------------------

@echo off
setlocal

set BUILD_RESULT=1

:: Get path for current running script.
set RUN_BUILD_LLVM_MSBUILD_SCRIPT_PATH=%~dp0

:: Configure all known build arguments.
set VS_BUILD_ARGS=
set VS_TARGET=build
if /i "%~1" == "clean" (
    set VS_TARGET="clean"
)
shift

set VS_PLATFORM=x64
if /i "%~1" == "i686" (
    set VS_PLATFORM="Win32"
)
if /i "%~1" == "win32" (
    set VS_PLATFORM="Win32"
)
shift

set VS_CONFIGURATION=Release
if /i "%~1" == "debug" (
    set VS_CONFIGURATION="Debug"
)
shift

set MONO_MSVC_SOURCE_DIR=%RUN_BUILD_LLVM_MSBUILD_SCRIPT_PATH%..\msvc\
if not "%~1" == "" (
    set MONO_MSVC_SOURCE_DIR=%~1
)
shift

set MONO_LLVM_BUILD_DIR=
if not "%~1" == "" (
    set MONO_LLVM_BUILD_DIR=%~1
)
shift

set MONO_LLVM_INSTALL_DIR=
if not "%~1" == "" (
    set MONO_LLVM_INSTALL_DIR=%~1
)
shift

set VS_ADDITIONAL_ARGUMENTS=
if not "%~1" == "" (
    set VS_ADDITIONAL_ARGUMENTS=%~1
)

:: Setup Windows environment.
call %MONO_MSVC_SOURCE_DIR%setup-windows-env.bat

:: Setup VS msbuild environment.
call %MONO_MSVC_SOURCE_DIR%setup-vs-msbuild-env.bat

if "%VS_ADDITIONAL_ARGUMENTS%" == "" (
    set "VS_ADDITIONAL_ARGUMENTS=/p:PlatformToolset=%VS_DEFAULT_PLATFORM_TOOL_SET% /p:MONO_TARGET_GC=sgen"
)

if not "%MONO_LLVM_BUILD_DIR%" == "" (
    set VS_BUILD_ARGS=/p:_LLVMBuildDir="%MONO_LLVM_BUILD_DIR%"
)

if not "%MONO_LLVM_INSTALL_DIR%" == "" (
    set VS_BUILD_ARGS=%VS_BUILD_ARGS% /p:_LLVMInstallDir="%MONO_LLVM_INSTALL_DIR%"
)

set VS_BUILD_ARGS=%VS_BUILD_ARGS% /p:MONO_ENABLE_LLVM=true /p:Configuration=%VS_CONFIGURATION% /p:Platform=%VS_PLATFORM% %VS_ADDITIONAL_ARGUMENTS% /t:%VS_TARGET% /m
call msbuild.exe %VS_BUILD_ARGS% "%MONO_MSVC_SOURCE_DIR%build-external-llvm.vcxproj" && (
    set BUILD_RESULT=0
) || (
    set BUILD_RESULT=1
    if not %ERRORLEVEL% == 0 (
        set BUILD_RESULT=%ERRORLEVEL%
    )
)

exit /b %BUILD_RESULT%

@echo on