#!/bin/bash
set -e
set -u

# &>> is used to pipe both stdout and stderr to a file in append mode
# Tee is used to both append full stdout to a log file and then print filtered stdout. 
# Before that we need to merge stdout and stderr using 2>&1.
# Grep ^\>\>\> is used to only print result messages from the benchmark.

echo \# Building benchmark $1...
rm -f run-benchmark-all.$1.log
dotnet build $1 &>>run-benchmark-all.$1.log
rm -f $1/bin/Debug/netcoreapp2.1/*.so
echo \# Running benchmark $1... | tee -a run-benchmark-all.$1.log
echo \# .NET Core | tee -a run-benchmark-all.$1.log
dotnet run --project=$1 | tee -a run-benchmark-all.$1.log | grep ^\>\>\>
echo \# Mono \(JIT\) | tee -a run-benchmark-all.$1.log
mono $1/bin/Debug/netcoreapp2.1/$1.dll 2>&1 | tee -a run-benchmark-all.$1.log | grep ^\>\>\>
echo \# Mono \(interpreter\) | tee -a run-benchmark-all.$1.log
mono --interpreter --interp=interp-only $1/bin/Debug/netcoreapp2.1/$1.dll 2>&1 | tee -a run-benchmark-all.$1.log | grep ^\>\>\>
echo \# Mono \(full AOT\) | tee -a run-benchmark-all.$1.log
mono --aot=full $1/bin/Debug/netcoreapp2.1/$1.dll &>>run-benchmark-all.$1.log
mono --full-aot $1/bin/Debug/netcoreapp2.1/$1.dll 2>&1 | tee -a run-benchmark-all.$1.log | grep ^\>\>\>
echo \# WebAssembly \(interpreter, node.js\) | tee -a run-benchmark-all.$1.log
mono ../packager.exe --out=wasm/$1 $1/bin/Debug/netcoreapp2.1/$1.dll &>>run-benchmark-all.$1.log
node test-runner.js $1 wasm/$1 2>&1 | tee -a run-benchmark-all.$1.log | grep \>\>\>
echo \# Test run complete. Check run-benchmark-all.$1.log for any errors.
