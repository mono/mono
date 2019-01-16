#!/usr/bin/env bash
set -e

if [ -x "$(command -v mono)" ] ; then
    if test -f $1; then
        echo . > /tmp/vbcs-log.txt
        RoslynCommandLineLogFile=/tmp/vbcs-log.txt mono --gc-params=nursery-size=64m $1 -pipename:$2 &
        serverpid=$!
        echo Compiler server started with PID $serverpid.
    else
        echo No compiler server found at location $1.
    fi;
else
    echo No 'mono' in path so cannot start compiler server.
fi;