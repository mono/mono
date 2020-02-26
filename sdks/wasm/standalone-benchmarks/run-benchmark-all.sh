#!/bin/bash
set -e
set -u

# &>> is used to pipe both stdout and stderr to a file in append mode
# Tee is used to both append full stdout to a log file and then print filtered stdout. 
# Before that we need to merge stdout and stderr using 2>&1.
# Grep ^\>\>\> is used to only print result messages from the benchmark.

LOGFILE=$(realpath run-benchmark-all.$1.log)
PACKAGER=$(realpath ../packager.exe)
SRC_DIR=$(realpath $1/bin/Debug/netcoreapp2.1)
OUT_DIR=$(realpath wasm/$1)
AOT_OUT_DIR=$(realpath wasm-aot/$1)
MONO_SDK_DIR=$(realpath ../../out)
EMSCRIPTEN_SDK_DIR=$(realpath ../../builds/toolchains/emsdk)
TEMPLATE_PATH=$(realpath ../runtime.js)

echo \# Building benchmark $1...
rm -f $LOGFILE
dotnet build $1 &>>$LOGFILE
rm -f SRC_DIR/*.so
echo \# Running benchmark $1... | tee -a $LOGFILE
echo \# .NET Core | tee -a $LOGFILE
dotnet run --project=$1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(JIT\) | tee -a $LOGFILE
mono $SRC_DIR/$1.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(interpreter\) | tee -a $LOGFILE
mono --interpreter --interp=interp-only $SRC_DIR/$1.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(full AOT\) | tee -a $LOGFILE
mono --aot=full $SRC_DIR/$1.dll &>>$LOGFILE
mono $SRC_DIR/$1.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# WebAssembly \(interpreter, node.js\) | tee -a $LOGFILE
mono --debug $PACKAGER --out=$OUT_DIR $SRC_DIR/$1.dll --template=$TEMPLATE_PATH &>>$LOGFILE
node test-runner.js $1 wasm/$1 2>&1 | tee -a $LOGFILE | grep \>\>\>
echo \# WebAssembly \(AOT, node.js\) | tee -a $LOGFILE
mono --debug $PACKAGER --linker --aot --builddir=$AOT_OUT_DIR/obj --appdir=$AOT_OUT_DIR $SRC_DIR/$1.dll --mono-sdkdir=$MONO_SDK_DIR --emscripten-sdkdir=$EMSCRIPTEN_SDK_DIR --template=$TEMPLATE_PATH
ninja -v -C $AOT_OUT_DIR/obj &>>$LOGFILE
node test-runner.js $1 $AOT_OUT_DIR 2>&1 | tee -a $LOGFILE | grep \>\>\>
echo \# Test run complete. Check $LOGFILE for any errors.
