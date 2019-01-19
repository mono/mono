#!/usr/bin/env bash
# usage: <mono path> <vbcscompiler.exe path> <log path> <pipename>

set -e

if test -f $1; then
    if test -f $2; then
        echo . > $3
        RoslynCommandLineLogFile=$3 mono --gc-params=nursery-size=64m $2 -pipename:$4 &
        serverpid=$!
        echo Compiler server started with PID $serverpid.
    else
        echo No compiler server found at $2.
    fi;
else
    echo mono not found at $1 so cannot start compiler server.
fi;