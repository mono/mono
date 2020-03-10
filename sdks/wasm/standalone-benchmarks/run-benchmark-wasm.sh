#!/bin/bash
set -e
set -x
set -u
function realpath { echo $(cd $(dirname ${1}); pwd)/$(basename ${1}); }


RUNTIME_DIR="../builds"
BENCHMARK=""
while (( "$#" )); do
  case "$1" in
    --wasm-runtime-path=*)
      RUNTIME_DIR="${1#*=}"
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

PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $BENCHMARK/bin/Debug/netcoreapp2.1)
TEMPLATE_PATH=$(realpath ../runtime.js)

echo "Using runtime $RUNTIME_DIR"
dotnet build $BENCHMARK
mono --debug $PACKAGER --wasm-runtime-path=$RUNTIME_DIR --out=wasm/$BENCHMARK $SRC_DIR/$BENCHMARK.dll --template=$TEMPLATE_PATH
node test-runner.js $BENCHMARK wasm/$BENCHMARK
