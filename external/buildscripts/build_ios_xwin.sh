#!/bin/sh

# The build configs are looking for this file rather than the perl script.
# easier to add this 

BASEDIR=$(dirname $0)

# Note : Not Implemented yet.  Script is here to make the katana build pass so that the mono build artifact is created
#  Uncomment the line below when ready to implement.  Remove the mkdir
if [ -d builds ]; then
	echo "Skip making builds directory.  Already exists"
else
	mkdir builds
fi

touch builds/dummy_ios_win.txt


#perl "$BASEDIR/build_ios_xwin.pl" "$@" || exit 1
