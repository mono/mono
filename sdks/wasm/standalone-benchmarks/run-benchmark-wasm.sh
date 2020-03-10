#!/bin/bash
set -e
set -x
set -u
function realpath { echo $(cd $(dirname ${1}); pwd)/$(basename ${1}); }

BENCHMARK=$(basename $1)

PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $BENCHMARK/bin/Debug/netcoreapp2.1)
TEMPLATE_PATH=$(realpath ../runtime.js)

dotnet build $BENCHMARK
mono --debug $PACKAGER --out=wasm/$BENCHMARK $SRC_DIR/$BENCHMARK.dll --template=$TEMPLATE_PATH
node test-runner.js $BENCHMARK wasm/$BENCHMARK
