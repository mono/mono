#!/bin/bash

# Arguments:
#-------------------------------------------------------
# $1 Visual Studio target, build|clean, default build
# $2 Host CPU architecture, x86_64|i686, default x86_64
#-------------------------------------------------------

VS_TARGET="build"
VS_PLATFORM="x64"

if [[ $1 = "clean" ]]; then
    VS_TARGET="clean"
fi

if [[ $2 = "i686" ]]; then
    VS_PLATFORM="Win32"
fi

VS_BUILD_ARGS="run-msbuild.bat"
VS_BUILD_ARGS+=" /p:Configuration=Release"
VS_BUILD_ARGS+=" /p:Platform=$VS_PLATFORM"
VS_BUILD_ARGS+=" /p:MONO_TARGET_GC=sgen"
VS_BUILD_ARGS+=" /t:Build"

export PATH=$ORIGINAL_PATH
if $WINDIR/System32/cmd.exe /c "$VS_BUILD_ARGS" ; then
    ./build/sgen/$VS_PLATFORM/bin/Release/mono-sgen.exe --version
else
    echo "Error, failed to build Visual Studio Mono runtime."
    exit 1
fi