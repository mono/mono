#!/usr/bin/env bash
# usage: start-compiler-server.sh <working directory> <log path> <pipename>
# ensure that VBCS_RUNTIME and VBCS_LOCATION environment variables are set.

set -u
set -e

if [ -s "$VBCS_LOCATION" ]; then
    CMD="RoslynCommandLineLogFile=$2 $VBCS_RUNTIME --gc-params=nursery-size=64m \"$VBCS_LOCATION\" -pipename:$3 &"
    echo "Log location set to $2"
    touch "$2"
    echo "cd $1; bash -c \"$CMD\""
    cd "$1"
    bash -c "$CMD"
    RESULT=$?
    if [ $RESULT -eq 0 ]; then
        echo Compiler server started.
    else
        echo Failed to start compiler server.
    fi;
else
    echo No compiler server found at path "$VBCS_LOCATION". Ensure that VBCS_LOCATION is set in config.make or passed as a parameter to make.
    echo Use ENABLE_COMPILER_SERVER=0 to disable the use of the compiler server and continue to build.
    exit 1
fi;
