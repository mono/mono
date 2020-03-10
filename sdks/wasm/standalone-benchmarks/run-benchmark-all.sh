#!/bin/bash
set -e
set -u

# &>> is used to pipe both stdout and stderr to a file in append mode
# Tee is used to both append full stdout to a log file and then print filtered stdout.
# Before that we need to merge stdout and stderr using 2>&1.
# Grep ^\>\>\> is used to only print result messages from the benchmark.
function realpath { echo $(cd $(dirname "$1"); pwd)/$(basename "$1"); }

BENCHMARK=$(basename "$1")

LOGFILE=$(realpath run-benchmark-all.$BENCHMARK.log)
PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $BENCHMARK/bin/Debug/netcoreapp2.1)
OUT_DIR=$(realpath wasm/$BENCHMARK)
AOT_OUT_DIR=$(realpath wasm-aot/$BENCHMARK)
MONO_SDK_DIR=$(realpath ../../out)
EMSCRIPTEN_SDK_DIR=$(realpath ../../builds/toolchains/emsdk)
TEMPLATE_PATH=$(realpath ../runtime.js)


echo \# Building benchmark $BENCHMARK...
rm -f $LOGFILE
dotnet build $BENCHMARK >> $LOGFILE 2>&1
rm -f SRC_DIR/*.so
echo \# Running benchmark $BENCHMARK... | tee -a $LOGFILE
echo \# .NET Core | tee -a $LOGFILE
dotnet run --project=$BENCHMARK | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(JIT\) | tee -a $LOGFILE
mono $SRC_DIR/$BENCHMARK.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(interpreter\) | tee -a $LOGFILE
mono --interpreter --interp=interp-only $SRC_DIR/$BENCHMARK.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(full AOT\) | tee -a $LOGFILE
mono --aot=full $SRC_DIR/$BENCHMARK.dll >>$LOGFILE 2>&1
mono $SRC_DIR/$BENCHMARK.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# WebAssembly \(interpreter, node.js\) | tee -a $LOGFILE
mono --debug $PACKAGER --out=$OUT_DIR $SRC_DIR/$BENCHMARK.dll --template=$TEMPLATE_PATH >>$LOGFILE 2>&1
node test-runner.js $BENCHMARK wasm/$BENCHMARK 2>&1 | tee -a $LOGFILE | grep \>\>\>
echo \# WebAssembly \(AOT, node.js\) | tee -a $LOGFILE
mono --debug $PACKAGER --linker --aot --builddir=$AOT_OUT_DIR/obj --appdir=$AOT_OUT_DIR $SRC_DIR/$BENCHMARK.dll --mono-sdkdir=$MONO_SDK_DIR --emscripten-sdkdir=$EMSCRIPTEN_SDK_DIR --template=$TEMPLATE_PATH
ninja -v -C $AOT_OUT_DIR/obj >> $LOGFILE 2>&1
node test-runner.js $BENCHMARK $AOT_OUT_DIR 2>&1 | tee -a $LOGFILE | grep \>\>\>
echo \# Test run complete. Check $LOGFILE for any errors.
