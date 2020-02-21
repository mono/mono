#!/bin/bash
set -e
set -x
set -u

dotnet build --project $1
mono ../packager.exe --out=wasm/$1 $1/bin/Debug/netcoreapp2.1/$1.dll
node test-runner.js $1 wasm/$1
