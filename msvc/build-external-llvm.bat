:: --------------------------------------------------
:: Run full LLVM build using msvc toolchain and available cmake generator.
:: Script needs to be run from within a matching build environment, x86|x64.
:: When executed from withing Visual Studio build environment current
:: build environment will be inherited by script.
::
:: %1 LLVM source root directory.
:: %2 LLVM build root directory.
:: %3 LLVM install root directory.
:: %4 Mono distribution root directory.
:: %5 VS CFLAGS.
:: %6 VS platform (Win32/x64)
:: %7 VS configuration (Debug/Release)
:: %8 VS target
:: %9 MsBuild bin path, if used.
:: --------------------------------------------------

@echo off
setlocal

set TEMP_PATH=%PATH%
set BUILD_RESULT=1

set CL_BIN_NAME=cl.exe
set LINK_BIN_NAME=link.exe
set GIT_BIN_NAME=git.exe
set CMAKE_BIN_NAME=cmake.exe
set NINJA_BIN_NAME=ninja.exe
set PYTHON_BIN_NAME=python.exe

set LLVM_DIR=%~1
set LLVM_BUILD_DIR=%~2
set LLVM_INSTALL_DIR=%~3
set MONO_DIST_DIR=%~4
set VS_CFLAGS=%~5
set VS_PLATFORM=%~6
set VS_CONFIGURATION=%~7
set VS_TARGET=%~8
set MSBUILD_BIN_PATH=%~9

:: Setup toolchain.
:: set GIT=
:: set CMAKE=
:: set NINJA=
set MSBUILD=%MSBUILD_BIN_PATH%msbuild.exe

if "%LLVM_DIR%" == "" (
    echo Missing LLVM source directory argument.
    goto ECHO_USAGE
)

if "%LLVM_BUILD_DIR%" == "" (
    echo Missing LLVM build directory argument.
    goto ECHO_USAGE
)

if "%LLVM_INSTALL_DIR%" == "" (
    echo Missing LLVM install directory argument.
    goto ECHO_USAGE
)

if "%MONO_DIST_DIR%" == "" (
    echo Missing Mono dist directory argument.
    goto ECHO_USAGE
)

if "%VS_CFLAGS%" == "" (
    echo Missing CFLAGS argument.
    goto ECHO_USAGE
)

if "%VS_PLATFORM%" == "" (
    set VS_PLATFORM=x64
)

if "%VS_CONFIGURATION%" == "" (
    set VS_CONFIGURATION=Release
)

if "%VS_TARGET%" == "" (
    set VS_TARGET=Build
)

if not exist "%LLVM_DIR%" (
    echo Could not find "%LLVM_DIR%".
    goto ON_ERROR
)

set LLVM_CFLAGS=%VS_CFLAGS%
set LLVM_ARCH=x86_64
if /i "%VS_PLATFORM%" == "win32" (
    set LLVM_ARCH=i386
)

:: Check if executed from VS2015/VS2017 build environment.
if "%VisualStudioVersion%" == "14.0" (
    goto ON_ENV_OK
)

if "%VisualStudioVersion%" == "15.0" (
    goto ON_ENV_OK
)

:: Executed outside VS2015/VS2017 build environment, try to locate Visual Studio C/C++ compiler and linker.
call :FIND_PROGRAM "" "%CL_BIN_NAME%" CL_PATH
if "%CL_PATH%" == "" (
    goto ON_ENV_WARNING
)

call :FIND_PROGRAM "" "%LINK_BIN_NAME%" LINK_PATH
if "%LINK_PATH%" == "" (
    goto ON_ENV_WARNING
)

goto ON_ENV_OK

:ON_ENV_WARNING

:: VS 2015.
set VC_VARS_ALL_FILE=%ProgramFiles(x86)%\Microsoft Visual Studio 14.0\VC\vcvarsall.bat
IF EXIST "%VC_VARS_ALL_FILE%" (
    echo For VS2015 builds, make sure to run this from within Visual Studio build or using "VS2015 x86|x64 Native Tools Command Prompt" command prompt.
	echo Setup a "VS2015 x86|x64 Native Tools Command Prompt" command prompt by using "%VC_VARS_ALL_FILE% x86|amd64".
)

:: VS 2017.
set VSWHERE_TOOLS_BIN=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe
if exist "%VSWHERE_TOOLS_BIN%" (
    echo For VS2017 builds, make sure to run this from within Visual Studio build or using "x86|x64 Native Tools Command Prompt for VS2017" command prompt.
	for /f "tokens=*" %%a IN ('"%VSWHERE_TOOLS_BIN%" -latest -property installationPath') do (
		echo Setup a "x86|x64 Native Tools Command Prompt for VS2017" command prompt by using "%%a\VC\Auxiliary\Build\vcvars32.bat|vcvars64.bat".
	)
)

echo Could not detect Visual Studio build environment. You may experience build problems if wrong toolchain is auto detected.

:ON_ENV_OK

:: Setup all cmake related generator, tools and variables.
call :SETUP_CMAKE_ENVIRONMENT
if "%CMAKE%" == "" (
    echo Failed to located working %CMAKE_BIN_NAME%, needs to be accessible in PATH or set using CMAKE environment variable.
    goto ON_ERROR
)

if "%CMAKE_GENERATOR%" == "" (
    echo Failed to setup cmake generator.
    goto ON_ERROR
)

:: Check target.
if /i "%VS_TARGET%" == "build" (
    goto ON_BUILD_LLVM
)

if /i "%VS_TARGET%" == "install" (
    goto ON_INSTALL_LLVM
)

if /i "%VS_TARGET%" == "clean" (
    goto ON_CLEAN_LLVM
)

:ON_BUILD_LLVM

:: If not set by caller, check environment for working git.exe.
call :FIND_PROGRAM "%GIT%" "%GIT_BIN_NAME%" GIT
if "%GIT%" == "" (
    echo Failed to located working %GIT_BIN_NAME%, needs to be accessible in PATH or set using GIT environment variable.
    goto ON_ERROR
)

:: Make sure llvm submodule is up to date.
pushd
cd "%LLVM_DIR%"
"%GIT%" submodule update --init
if not ERRORLEVEL == 0 (
   "%GIT%" submodule init
    "%GIT%" submodule update
    if not ERRORLEVEL == 0 (
        echo Git llvm submodules failed to updated. You may experience compilation problems if some submodules are out of date.
    )
)
popd

if not exist "%LLVM_BUILD_DIR%" (
    mkdir "%LLVM_BUILD_DIR%"
)

cd "%LLVM_BUILD_DIR%"

:: Make sure cmake pick up msvc toolchain regardless of selected generator (Visual Studio|Ninja)
set CC=%CL_BIN_NAME%
set CXX=%CL_BIN_NAME%

set CMAKE_GENERATOR_ARGS=
if /i "%CMAKE_GENERATOR%" == "ninja" (
    set CMAKE_GENERATOR_ARGS=-DCMAKE_BUILD_TYPE=%VS_CONFIGURATION%
) else (
    set CMAKE_GENERATOR_ARGS=-Thost=x64
)

:: Run cmake.
"%CMAKE%" ^
-DCMAKE_INSTALL_PREFIX="%LLVM_INSTALL_DIR%" ^
-DLLVM_TARGETS_TO_BUILD="X86" ^
-DLLVM_BUILD_TESTS=Off ^
-DLLVM_INCLUDE_TESTS=Off ^
-DLLVM_BUILD_EXAMPLES=Off ^
-DLLVM_INCLUDE_EXAMPLES=Off ^
-DLLVM_TOOLS_TO_BUILD="opt;llc;llvm-config;llvm-dis;llvm-mc" ^
-DLLVM_ENABLE_LIBXML2=Off ^
-DCMAKE_SYSTEM_PROCESSOR="%LLVM_ARCH%" ^
%CMAKE_GENERATOR_ARGS% ^
-G "%CMAKE_GENERATOR%" ^
"%LLVM_DIR%"

if not ERRORLEVEL == 0 (
    goto ON_ERROR
)

if /i "%CMAKE_GENERATOR%" == "ninja" (
    :: Build LLVM using ninja build system.
    call "%NINJA%" -j4 || (
        goto ON_ERROR
    )
) else (
    :: Build LLVM using msbuild build system.
    call "%MSBUILD%" llvm.sln /p:Configuration=%VS_CONFIGURATION% /p:Platform=%VS_PLATFORM% /t:%VS_TARGET% /v:m /nologo || (
        goto ON_ERROR
    )
)

:ON_INSTALL_LLVM

:: Make sure build install folder exists.
if not exist "%LLVM_INSTALL_DIR%" (
    echo Could not find "%LLVM_INSTALL_DIR%", creating folder for build output.
    mkdir "%LLVM_INSTALL_DIR%"
)

:: Make sure Mono dist folder exists.
if not exist "%MONO_DIST_DIR%" (
    echo Could not find "%MONO_DIST_DIR%", creating folder for build output.
    mkdir "%MONO_DIST_DIR%"
)

if exist "%LLVM_BUILD_DIR%\build.ninja" (
    pushd
    cd "%LLVM_BUILD_DIR%"
    call "%NINJA%" install
    popd
)

if exist "%LLVM_BUILD_DIR%\install.vcxproj" (
    "%MSBUILD%" "%LLVM_BUILD_DIR%\install.vcxproj" /p:Configuration=%VS_CONFIGURATION% /p:Platform=%VS_PLATFORM% /v:m /nologo
)

if not exist "%LLVM_INSTALL_DIR%\bin\opt.exe" (
    echo Missing LLVM build output, "%LLVM_INSTALL_DIR%\bin\opt.exe"
    goto ON_ERROR
)

if not exist "%LLVM_INSTALL_DIR%\bin\llc.exe" (
    echo Missing LLVM build output, "%LLVM_INSTALL_DIR%\bin\llc.exe"
    goto ON_ERROR
)

copy /Y "%LLVM_INSTALL_DIR%\bin\opt.exe" "%MONO_DIST_DIR%" >nul 2>&1
copy /Y "%LLVM_INSTALL_DIR%\bin\llc.exe" "%MONO_DIST_DIR%" >nul 2>&1

goto ON_SUCCESS

:ON_CLEAN_LLVM

if exist "%LLVM_BUILD_DIR%\build.ninja" (
    pushd
    cd "%LLVM_BUILD_DIR%"
    call "%NINJA%" clean
    popd
)

if exist "%LLVM_BUILD_DIR%\llvm.sln" (
    "%MSBUILD%" "%LLVM_BUILD_DIR%\llvm.sln" /p:Configuration=%VS_CONFIGURATION% /p:Platform=%VS_PLATFORM% /t:Clean /v:m /nologo
)

goto ON_SUCCESS

:ON_SUCCESS

set BUILD_RESULT=0
goto ON_EXIT

:ECHO_USAGE:
    ECHO Usage: build-external-llvm.bat [llvm_src_dir] [llvm_build_dir] [llvm_install_dir] [mono_dist_dir] [vs_cflags] [vs_plaform] [vs_configuration].

:ON_ERROR
    echo Failed to build LLVM.
    goto ON_EXIT

:ON_EXIT
    set PATH=%TEMP_PATH%
    exit /b %BUILD_RESULT%

:: ##############################################################################################################################
:: Functions

:: --------------------------------------------------
:: Finds a program using environment.
::
:: %1 Existing program to check for.
:: %2 Name of binary to locate.
:: %3 Output, variable to set if found requested program.
:: --------------------------------------------------
:FIND_PROGRAM

:: If not set by caller, check environment for program.
if exist "%~1" (
    goto :EOF
)

call where /q "%~2" && (
    for /f "delims=" %%a in ('where "%~2"') do (
        set "%~3=%%a"
    )
) || (
    set "%~3="
)

goto :EOF

:: --------------------------------------------------
:: Setup up cmake build environment, including generator, build tools and variables.
:: --------------------------------------------------
:SETUP_CMAKE_ENVIRONMENT

:: If not set by caller, check environment for working cmake.exe.
call :FIND_PROGRAM "%CMAKE%" "%CMAKE_BIN_NAME%" CMAKE
if "%CMAKE%" == "" (
    goto _SETUP_CMAKE_ENVIRONMENT_EXIT
)

if /i "%VS_TARGET%" == "build" (
    echo Found CMake: %CMAKE%
)

:: Check for optional cmake generate and build tools.
call :FIND_PROGRAM "%NINJA%" "%NINJA_BIN_NAME%" NINJA

if not "%NINJA%" == "" (
    goto _SETUP_CMAKE_ENVIRONMENT_NINJA_GENERATOR
)

:_SETUP_CMAKE_ENVIRONMENT_VS_GENERATOR

if /i "%VS_TARGET%" == "build" (
    echo Using Visual Studio build generator.
)

:: Detect VS version to use right cmake generator.
set CMAKE_GENERATOR=Visual Studio 14 2015
if "%VisualStudioVersion%" == "15.0" (
    set CMAKE_GENERATOR=Visual Studio 15 2017
)

if /i "%VS_PLATFORM%" == "x64" (
    set CMAKE_GENERATOR=%CMAKE_GENERATOR% Win64
)

set LLVM_BUILD_OUTPUT_DIR=%LLVM_BUILD_DIR%\%VS_CONFIGURATION%

goto _SETUP_CMAKE_ENVIRONMENT_EXIT

:_SETUP_CMAKE_ENVIRONMENT_NINJA_GENERATOR

if /i "%VS_TARGET%" == "build" (
    echo Found Ninja: %NINJA%
    echo Using Ninja build generator.
)

set CMAKE_GENERATOR=Ninja
set LLVM_BUILD_OUTPUT_DIR=%LLVM_BUILD_DIR%

:_SETUP_CMAKE_ENVIRONMENT_EXIT

goto :EOF

@echo on
