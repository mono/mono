#!/bin/sh

# The build configs are looking for this file rather than the perl script.
# easier to add this 

BASEDIR=$(dirname $0)

perl "$BASEDIR/build_runtime_android.pl" "$@"
