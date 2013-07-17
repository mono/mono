#!/bin/bash

PREFIX=`pwd`/builds/stv

OUTDIR=builds/embedruntimes/stv

perl PrepareSTVNDK.pl -ndk=r03 -env=envsetup.sh && source envsetup.sh && source ${STV_NDK_ROOT}/stv-ndk-env.sh

CXXFLAGS="-DARM_FPU_VFP=1 -D__ARM_EABI__ -mno-thumb -march=armv7-a -mfpu=vfpv3 -mtune=cortex-a9 -fPIC";
CC="${STV_GCC_PREFIX}gcc"
CXX="${STV_GCC_PREFIX}g++"
AR="${STV_GCC_PREFIX}ar"
LD="${STV_GCC_PREFIX}ld"
LDFLAGS=""

CONFIG_OPTS="\
--prefix=$PREFIX \
--cache-file=stv_cross.cache \
--host=arm-unknown-linux-gnueabi \
--disable-mcs-build \
--disable-parallel-mark \
--disable-shared-handles \
--with-sigaltstack=no \
--with-tls=pthread \
--with-glib=embedded \
--disable-nls \
mono_cv_uscore=yes"

make clean && make distclean
rm stv_cross.cache

pushd eglib
autoreconf -i
popd
autoreconf -i

# Run configure
./configure $CONFIG_OPTS CFLAGS="$CXXFLAGS" CXXFLAGS="$CXXFLAGS" LDFLAGS="$LDFLAGS" CC="$CC" CXX="$CXX" AR="$AR" LD="$LD"

# Run Make
make && echo "Build SUCCESS!" || exit 1

rm -rf builds

mkdir -p $OUTDIR
cp -f mono/mini/.libs/libmono.a $OUTDIR

if [ -d builds/monodistribution ] ; then
rm -r builds/monodistribution
fi
