#!/bin/bash
set -e
set -x
set -u

PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $1/bin/Debug/netcoreapp2.1)
AOT_OUT_DIR=$(realpath wasm-aot/$1)
MONO_SDK_DIR=$(realpath ../../out)
EMSCRIPTEN_SDK_DIR=$(realpath ../../builds/toolchains/emsdk)
TEMPLATE_PATH=$(realpath ../runtime.js)

dotnet build $1
mono --debug $PACKAGER --linker --aot --builddir=$AOT_OUT_DIR/obj --appdir=$AOT_OUT_DIR $SRC_DIR/$1.dll --mono-sdkdir=$MONO_SDK_DIR --emscripten-sdkdir=$EMSCRIPTEN_SDK_DIR --template=$TEMPLATE_PATH
ninja -v -C $AOT_OUT_DIR/obj
node test-runner.js $1 $AOT_OUT_DIR
