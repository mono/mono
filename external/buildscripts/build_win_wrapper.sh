#!/bin/sh

# Need to put the cygwin stuff into our PATH
export PATH=/usr/bin:$PATH
BUILD_SCRIPT_ROOT=$(dirname $0)

echo ">>> Build Script Root = $BUILD_SCRIPT_ROOT"
echo
echo ">>> PATH in Win Shell Script = $PATH"
echo

perl "$BUILD_SCRIPT_ROOT/build.pl" "$@"