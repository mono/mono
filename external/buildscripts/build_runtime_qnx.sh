#!/bin/bash

# Set up QNX dev environment
# Taken from/opt/bbndk/NativeSDK-env-1_0_7.2942.sh

PREFIX=`pwd`/../builds/qnx

OUTDIR=../builds/embedruntimes/qnx
BUILDSCRIPTSDIR=external/buildscripts

perl ${BUILDSCRIPTSDIR}/PrepareBB10NDK.pl -ndk=r09 -env=envsetup.sh && source envsetup.sh

source $BB10_NDK_ROOT/bbndk-env.sh

make clean && make distclean

rm -r *.cache config.status nto-arm-le-v7 libgc/config.status autom4te.cache Makefile

NOCONFIGURE=1 ./autogen.sh
cd eglib; NOCONFIGURE=1 ./autogen.sh

cd ..
addvariant nto arm le-v7
cd nto-arm-le-v7

# Run Make
make && echo "Build SUCCESS!" || exit 1

rm -rf ../builds

mkdir -p $OUTDIR
cp -f mono/mini/.libs/libmono.a $OUTDIR

if [ -d ../builds/monodistribution ] ; then
rm -r ../builds/monodistribution
fi

# Clean up for next build
cd ..
make clean && make distclean
rm Makefile

