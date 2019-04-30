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

BUILD_LLVM_MSBUILD_SCRIPT_PATH=$(cd "$(dirname "$0")"; pwd)

BUILD_LLVM_MSBUILD_SCRIPT_PATH=$(cygpath -w "$BUILD_LLVM_MSBUILD_SCRIPT_PATH/build_llvm_msbuild.bat")
MONO_MSVC_SOURCE_DIR=$(cygpath -w "$4")
MONO_LLVM_BUILD_DIR=$(cygpath -w "$5")
MONO_LLVM_INSTALL_DIR=$(cygpath -w "$6")

"$WINDIR/System32/cmd.exe" /c "$BUILD_LLVM_MSBUILD_SCRIPT_PATH" "$1" "$2" "$3" "$MONO_MSVC_SOURCE_DIR" "$MONO_LLVM_BUILD_DIR" "$MONO_LLVM_INSTALL_DIR" "$7"
