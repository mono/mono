#!/usr/bin/env bash
# usage: <mono path> <vbcscompiler.exe path> <log path> <pipename>

set -e

if test -f $2; then
    echo . > $3
    echo "RoslynCommandLineLogFile=$3 $1 --gc-params=nursery-size=64m $2 -pipename:$4 &"
    RoslynCommandLineLogFile=$3 $1 --gc-params=nursery-size=64m $2 -pipename:$4 &
    serverpid=$!
    echo Compiler server started with PID $serverpid.
else
    echo No compiler server found at $2.
fi;