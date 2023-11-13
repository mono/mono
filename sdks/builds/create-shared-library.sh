#!/bin/bash -e

# This script takes a static library (.a) as input and creates a dylib or framework from it:
# 1) First all the object files are extracted from the static library.
# 2) Then all the object files are relinked into a dynamic library / framework).
#
# Arguments
# 1: the input static library
# 2: the output framework library
# 3+: any custom linker arguments.

STATIC_LIBRARY=$1
FRAMEWORK=$2

if test -z $STATIC_LIBRARY; then
	echo "The first argument must be the input (static) library."
	exit 1
elif ! test -f $STATIC_LIBRARY; then
	echo "Could not find the input library: $STATIC_LIBRARY"
	exit 1
fi

if test -z $FRAMEWORK; then
	echo "The second argument must be the output (shared) library."
	exit 1
fi

shift 2
LINKER_FLAGS="$@"

mkdir -p $(dirname $FRAMEWORK)
TMPDIR=$FRAMEWORK.tmpdir
rm -Rf $TMPDIR
mkdir -p $TMPDIR
cd $TMPDIR
ar -x $STATIC_LIBRARY
cd ..

$CC $LINKER_FLAGS -dynamiclib -O2 -Wl,-application_extension -compatibility_version 2 -current_version 2.0 -framework CoreFoundation -lobjc -liconv -lz -o $FRAMEWORK $TMPDIR/*.o
