#!/bin/sh

# The build configs are looking for this file rather than the perl script.
# easier to add this 

BASEDIR=$(dirname $0)

if [ "x$1" == "x--runtime-only" ]; then
	perl "$BASEDIR/build_runtime_iphone.pl" "--runtime=1"|| exit 1
elif [ "x$1" == "x--xcomp-only" ]; then
	perl "$BASEDIR/build_runtime_iphone.pl" "--xcomp=1" || exit 1
elif [ "x$1" == "x--simulator-only" ]; then
	perl "$BASEDIR/build_runtime_iphone.pl" "--simulator=1" || exit 1
else
	perl "$BASEDIR/build_runtime_iphone.pl" "$@" || exit 1
fi
