#!/bin/bash
set -e
set -x
set -u
function realpath { echo $(cd $(dirname ${1}); pwd)/$(basename ${1}); }

BENCHMARK=""
while (( "$#" )); do
  case "$1" in
    --wasm-runtime-path=*)
      RUNTIME_DIR="${1#*=}"
      shift
      ;;
    --wasm-package=*)
      MONO_PACKAGE="${1#*=}"
      shift
      ;;
    -*|--*=)
      echo "Unknown argument $1" >&2
      exit 1
      ;;
    *)
      BENCHMARK=$(basename $1)
      shift
      ;;
  esac
done


if [ ! -z ${MONO_PACKAGE+x} ]; then
    ZIP_DIR=${RUNTIME_DIR:=$(basename ${MONO_PACKAGE%.*})}
    unzip -u -d $ZIP_DIR $MONO_PACKAGE "builds/**"
    RUNTIME_DIR=$ZIP_DIR/builds
fi
RUNTIME_DIR=${RUNTIME_DIR:="../builds"}

PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $BENCHMARK/bin/Debug/netcoreapp2.1)
TEMPLATE_PATH=$(realpath ../runtime.js)

echo "Using runtime $RUNTIME_DIR"
dotnet build $BENCHMARK
mono --debug $PACKAGER --wasm-runtime-path=$RUNTIME_DIR --out=wasm/$BENCHMARK $SRC_DIR/$BENCHMARK.dll --template=$TEMPLATE_PATH
node test-runner.js $BENCHMARK wasm/$BENCHMARK
