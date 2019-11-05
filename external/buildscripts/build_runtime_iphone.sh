#!/bin/sh

# The build configs are looking for this file rather than the perl script.
# easier to add this 

BASEDIR=$(dirname $0)

if [ -d builds ]; then
	echo "Skip making builds directory.  Already exists"
else
	mkdir builds
fi

if [ "x$1" == "x--runtime-only" ]; then
	touch builds/dummy_iphone_runtime.txt
elif [ "x$1" == "x--xcomp-only" ]; then
	touch builds/dummy_iphone_xcomp.txt
elif [ "x$1" == "x--simulator-only" ]; then
	touch builds/dummy_iphone_sim.txt
fi
