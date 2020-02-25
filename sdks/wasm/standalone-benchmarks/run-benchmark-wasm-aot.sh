#!/bin/bash
set -e
set -x
set -u

SRCDIR=$(realpath $1/bin/Debug/netcoreapp2.1)
OUTDIR=$(realpath wasm-aot/$1)
EMSCRIPTEN_SDK_DIR=$(realpath ../../builds/toolchains/emsdk)

dotnet build $1
pushd ..
mono packager.exe --linker --aot --builddir=$OUTDIR/obj --appdir=$OUTDIR $SRCDIR/$1.dll --emscripten-sdkdir=$EMSCRIPTEN_SDK_DIR
pwd
ninja -v -C $OUTDIR/obj
popd
node test-runner.js $1 wasm-aot/$1
