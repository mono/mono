#!/bin/bash
set -e
set -x
set -u

PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $1/bin/Debug/netcoreapp2.1)
TEMPLATE_PATH=$(realpath ../runtime.js)

dotnet build $1
mono --debug $PACKAGER --out=wasm/$1 $SRC_DIR/$1.dll --template=$TEMPLATE_PATH
node test-runner.js $1 wasm/$1
