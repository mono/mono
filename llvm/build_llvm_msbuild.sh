#!/bin/bash

# Set up build environment and build LLVM using MSVC and msbuild targets in mono.sln.

# Arguments:
# -------------------------------------------------------
# $1 Visual Studio target, build|clean, default build
# $2 Host CPU architecture, x86_64|i686, default x86_64
# $3 Visual Studio configuration, debug|release, default release
# $4 Mono MSVC source folder.
# $5 LLVM build directory.
# $6 LLVM install directory.
# $7 Additional arguments passed to msbuild, needs to be quoted if multiple.
# -------------------------------------------------------

function win32_format_path {
    local formatted_path=$1
    local host_win32_wsl=0
    local host_win32_cygwin=0

    host_uname="$(uname -a)"
    case "$host_uname" in
        *Microsoft*)
            host_win32_wsl=1
            ;;
        CYGWIN*)
            host_win32_cygwin=1
            ;;
	esac

    if  [[ $host_win32_wsl = 1 ]] && [[ $1 == "/mnt/"* ]]; then
        formatted_path="$(wslpath -a -w "$1")"
    elif [[ $host_win32_cygwin = 1 ]] && [[ $1 == "/cygdrive/"* ]]; then
        formatted_path="$(cygpath -a -w "$1")"
    fi

    echo "$formatted_path"
}

BUILD_LLVM_MSBUILD_SCRIPT_PATH=$(cd "$(dirname "$0")"; pwd)

BUILD_LLVM_MSBUILD_SCRIPT_PATH=$(win32_format_path "$BUILD_LLVM_MSBUILD_SCRIPT_PATH/build_llvm_msbuild.bat")
MONO_MSVC_SOURCE_DIR=$(win32_format_path "$4")
MONO_LLVM_BUILD_DIR=$(win32_format_path "$5")
MONO_LLVM_INSTALL_DIR=$(win32_format_path "$6")

WINDOWS_CMD=$(which cmd.exe)
if [ ! -f $WINDOWS_CMD ]; then
    WINDOWS_CMD=$WINDIR/System32/cmd.exe
fi

"$WINDOWS_CMD" /c "$BUILD_LLVM_MSBUILD_SCRIPT_PATH" "$1" "$2" "$3" "$MONO_MSVC_SOURCE_DIR" "$MONO_LLVM_BUILD_DIR" "$MONO_LLVM_INSTALL_DIR" "$7"
