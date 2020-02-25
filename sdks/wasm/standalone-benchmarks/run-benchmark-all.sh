#!/bin/bash
set -e
set -u

# &>> is used to pipe both stdout and stderr to a file in append mode
# Tee is used to both append full stdout to a log file and then print filtered stdout. 
# Before that we need to merge stdout and stderr using 2>&1.
# Grep ^\>\>\> is used to only print result messages from the benchmark.

LOGFILE=$(realpath run-benchmark-all.$1.log)
SRCDIR=$(realpath $1/bin/Debug/netcoreapp2.1)
OUTDIR=$(realpath wasm-aot/$1)
EMSCRIPTEN_SDK_DIR=$(realpath ../../builds/toolchains/emsdk)


echo \# Building benchmark $1...
rm -f $LOGFILE
dotnet build $1 &>>$LOGFILE
rm -f SRCDIR/*.so
echo \# Running benchmark $1... | tee -a $LOGFILE
echo \# .NET Core | tee -a $LOGFILE
dotnet run --project=$1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(JIT\) | tee -a $LOGFILE
mono $1/bin/Debug/netcoreapp2.1/$1.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(interpreter\) | tee -a $LOGFILE
mono --interpreter --interp=interp-only $1/bin/Debug/netcoreapp2.1/$1.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# Mono \(full AOT\) | tee -a $LOGFILE
mono --aot=full $1/bin/Debug/netcoreapp2.1/$1.dll &>>$LOGFILE
mono $1/bin/Debug/netcoreapp2.1/$1.dll 2>&1 | tee -a $LOGFILE | grep ^\>\>\>
echo \# WebAssembly \(interpreter, node.js\) | tee -a $LOGFILE
mono ../packager.exe --out=wasm/$1 $1/bin/Debug/netcoreapp2.1/$1.dll &>>$LOGFILE
node test-runner.js $1 wasm/$1 2>&1 | tee -a $LOGFILE | grep \>\>\>
echo \# WebAssembly \(AOT, node.js\) | tee -a $LOGFILE
mono ../packager.exe --out=wasm/$1 $1/bin/Debug/netcoreapp2.1/$1.dll &>>$LOGFILE
node test-runner.js $1 wasm/$1 2>&1 | tee -a $LOGFILE | grep \>\>\>
echo \# Test run complete. Check $LOGFILE for any errors.
