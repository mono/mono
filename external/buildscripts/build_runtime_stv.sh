#!/bin/sh

# The build configs are looking for this file rather than the perl script.
# easier to add this 

# Note : Not Implemented yet.  Script is here to make the katana build pass so that the mono build artifact is created
if [ -d builds ]; then
	echo "Skip making builds directory.  Already exists"
else
	mkdir builds
fi

touch builds/dummy_stv.txt

