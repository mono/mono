#!/bin/bash
set -e
set -x
set -u

function realpath { echo $(cd $(dirname "$1"}); pwd)/$(basename "$1"); }

BENCHMARK=$(basename $1)

PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $BENCHMARK/bin/Debug/netcoreapp2.1)
AOT_OUT_DIR=$(realpath wasm-aot/$BENCHMARK)
MONO_SDK_DIR=$(realpath ../../out)
EMSCRIPTEN_SDK_DIR=$(realpath ../../builds/toolchains/emsdk)
TEMPLATE_PATH=$(realpath ../runtime.js)

dotnet build $BENCHMARK
mono --debug $PACKAGER --linker --aot --builddir=$AOT_OUT_DIR/obj --appdir=$AOT_OUT_DIR $SRC_DIR/$BENCHMARK.dll --mono-sdkdir=$MONO_SDK_DIR --emscripten-sdkdir=$EMSCRIPTEN_SDK_DIR --template=$TEMPLATE_PATH
ninja -v -C $AOT_OUT_DIR/obj
node test-runner.js $BENCHMARK $AOT_OUT_DIR
