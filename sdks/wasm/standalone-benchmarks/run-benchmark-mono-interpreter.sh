#!/bin/bash
set -e
set -x
set -u

dotnet build $1
rm -f $1/bin/Debug/netcoreapp2.1/*.so
mono --interpreter --interp=interp-only $1/bin/Debug/netcoreapp2.1/$1.dll
