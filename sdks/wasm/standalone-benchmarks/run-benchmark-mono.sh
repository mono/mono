#!/bin/bash
set -e
set -x
set -u

BENCHMARK=$(basename $1)

dotnet build $BENCHMARK
rm -f $BENCHMARK/bin/Debug/netcoreapp2.1/*.so
mono $BENCHMARK/bin/Debug/netcoreapp2.1/$BENCHMARK.dll
