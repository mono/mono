#!/bin/bash

# First build for Tizen mobile devices
PREFIX="$PWD/builds/tizen"

BUILDDIR=/$PWD
OUTDIR=builds/embedruntimes/tizen
BUILDSCRIPTSDIR=external/buildscripts

perl ${BUILDSCRIPTSDIR}/SDKDownloader.pm --repo_name=tizen-sdk --artifacts_folder=artifacts && source artifacts/SDKDownloader/tizen-sdk/env.sh

CXXFLAGS="-Os -DHAVE_ARMV6=1 -DARM_FPU_VFP=1 -D__ARM_EABI__ -march=armv7-a -mfloat-abi=softfp -mfpu=neon -mtune=cortex-a9 \
-ffunction-sections -fdata-sections -fno-strict-aliasing -fPIC"
CFLAGS="$CXXFLAGS"

TIZEN_PREFIX=${TIZEN_SDK}/tools/arm-linux-gnueabi-gcc-4.9/bin/arm-linux-gnueabi-

CC="${TIZEN_PREFIX}gcc --sysroot=${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP} -I${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP}/usr/include -DTIZEN"
CXX="${TIZEN_PREFIX}g++ --sysroot=${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP} -I${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP}/usr/include -DTIZEN"
AR="${TIZEN_PREFIX}ar"
LD="${TIZEN_PREFIX}ld"
RANLIB="${TIZEN_PREFIX}ranlib"
STRIP="${TIZEN_PREFIX}strip"

CONFIG_OPTS="\
--prefix=$PREFIX \
--with-sysroot=${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP} \
--cache-file=tizen_cross.cache \
--host=arm-linux-gnueabi \
--disable-mcs-build \
--disable-parallel-mark \
--disable-shared-handles \
--with-sigaltstack=no \
--with-tls=pthread \
--with-glib=embedded \
--enable-nls=no \
mono_cv_uscore=yes"

LDFLAGS="-ldlog"

make clean && make distclean
rm tizen_cross.cache

pushd eglib
autoreconf -i
popd
autoreconf -i

# Run configure
./configure $CONFIG_OPTS CFLAGS="$CXXFLAGS" CXXFLAGS="$CXXFLAGS" LDFLAGS="$LDFLAGS" CC="$CC" CXX="$CXX" AR="$AR" LD="$LD" RANLIB="$RANLIB" STRIP="$STRIP"

# Run Make
make && echo "Build SUCCESS!" || exit 1

rm -rf $PWD/builds

mkdir -p $OUTDIR
cp -f mono/mini/.libs/libmono.a $OUTDIR

# Clean up for next build
make clean && make distclean

if [ -d builds/monodistribution ] ; then
rm -r builds/monodistribution
fi

# Now build for the Tizen emulator

make clean
rm tizen_cross.cache

OUTDIR=builds/embedruntimes/tizenemulator
CXXFLAGS="-Os -ffunction-sections -fdata-sections -fno-strict-aliasing -fPIC"
CFLAGS="$CXXFLAGS"

TIZEN_PREFIX=${TIZEN_SDK}/tools/i386-linux-gnueabi-gcc-4.9/bin/i386-linux-gnueabi-
TIZEN_ROOTSTRAP=mobile-2.4-emulator.core

CC="${TIZEN_PREFIX}gcc --sysroot=${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP} -I${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/rootstraps/${TIZEN_ROOTSTRAP}/usr/include -DTIZEN"
CXX="${TIZEN_PREFIX}g++ --sysroot=${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP} -I${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/rootstraps/${TIZEN_ROOTSTRAP}/usr/include -DTIZEN"
AR="${TIZEN_PREFIX}ar"
LD="${TIZEN_PREFIX}ld"
RANLIB="${TIZEN_PREFIX}ranlib"
STRIP="${TIZEN_PREFIX}strip"

CONFIG_OPTS="\
--prefix=$PREFIX \
--with-sysroot=${TIZEN_SDK}/platforms/${TIZEN_PLATFORM}/mobile/rootstraps/${TIZEN_ROOTSTRAP} \
--cache-file=tizen_cross.cache \
--host=i386-linux-gnueabi \
--disable-mcs-build \
--disable-parallel-mark \
--disable-shared-handles \
--with-sigaltstack=no \
--with-tls=pthread \
--with-glib=embedded \
--enable-nls=no \
mono_cv_uscore=yes"

pushd eglib
autoreconf -i
popd
autoreconf -i

# Run configure
./configure $CONFIG_OPTS CFLAGS="$CXXFLAGS" CXXFLAGS="$CXXFLAGS" LDFLAGS="$LDFLAGS" CC="$CC" CXX="$CXX" AR="$AR" LD="$LD" RANLIB="$RANLIB" STRIP="$STRIP"

# Run Make
make && echo "Build SUCCESS!" || exit 1

mkdir -p $OUTDIR
cp -f mono/mini/.libs/libmono.a $OUTDIR

# Clean up for next build
make clean && make distclean

if [ -d builds/monodistribution ] ; then
rm -r builds/monodistribution
fi
