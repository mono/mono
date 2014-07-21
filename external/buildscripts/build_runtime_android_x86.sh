#!/bin/bash

# NB! Prereq : ANDROID_NDK_ROOT=/usr/local/android-ndk-xxx or similar
# Todo: set appropriate ARM flags for hard floats

export ANDROID_PLATFORM=android-9
GCC_PREFIX=i686-linux-android-
GCC_VERSION=4.4.3
OUTDIR=builds/embedruntimes/android
PREFIX=`pwd`/builds/android

NDK_ROOT=`cd $ANDROID_NDK_ROOT && pwd`

if [ ! -f $NDK_ROOT/GNUmakefile ]; then
	echo "Failed to locate Android NDK; is ANDROID_NDK_ROOT correctly set?"
	exit 1
fi

HOST_ENV=`uname -s`
case "$HOST_ENV" in
    Darwin)
        HOST_ENV=darwin-x86
        ;;
    Linux)
        HOST_ENV=linux-x86
        ;;
    CYGWIN*|*_NT-*)
        HOST_ENV=windows
        ;;
	*)
		echo "Failed to locate supported host environment; HOST_ENV = $HOST_ENV ..."
		exit 1
		;;
esac

PLATFORM_ROOT=$NDK_ROOT/platforms/$ANDROID_PLATFORM/arch-x86
TOOLCHAIN=$NDK_ROOT/toolchains/x86-$GCC_VERSION/prebuilt/$HOST_ENV

if [ ! -a $TOOLCHAIN -o ! -a $PLATFORM_ROOT ]; then
	NDK_NAME=`basename $NDK_ROOT`
	echo "Failed to locate toolchain/platform; $NDK_NAME | $HOST_ENV | $GCC_VERSION | $ANDROID_PLATFORM"
	echo "Toolchain = $TOOLCHAIN"
	echo "Platform = $PLATFORM_ROOT"
	exit 1
fi

PATH="$TOOLCHAIN/bin:$PATH"
CC="$TOOLCHAIN/bin/${GCC_PREFIX}gcc --sysroot=$PLATFORM_ROOT"
CXX="$TOOLCHAIN/bin/${GCC_PREFIX}g++ --sysroot=$PLATFORM_ROOT"
CPP="$TOOLCHAIN/bin/${GCC_PREFIX}cpp"
CXXCPP="$TOOLCHAIN/bin/${GCC_PREFIX}cpp"
CPATH="$PLATFORM_ROOT/usr/include"
LD=$TOOLCHAIN/bin/${GCC_PREFIX}ld
AS=$TOOLCHAIN/bin/${GCC_PREFIX}as
AR=$TOOLCHAIN/bin/${GCC_PREFIX}ar
RANLIB=$TOOLCHAIN/bin/${GCC_PREFIX}ranlib
STRIP=$TOOLCHAIN/bin/${GCC_PREFIX}strip
CFLAGS="\
-DANDROID -DPLATFORM_ANDROID -DLINUX -D__linux__ \
-DHAVE_USR_INCLUDE_MALLOC_H -DPAGE_SIZE=0x1000 \
-D_POSIX_PATH_MAX=256 -DS_IWRITE=S_IWUSR \
-DHAVE_PTHREAD_MUTEX_TIMEDLOCK \
-fpic -g \
-ffunction-sections -fdata-sections"
CXXFLAGS=$CFLAGS
LDFLAGS="\
-Wl,--no-undefined \
-ldl -lm -llog -lc -lgcc"

CONFIG_OPTS="\
--prefix=$PREFIX \
--cache-file=android_cross.cache \
--host=i686-unknown-linux \
--disable-mcs-build \
--disable-parallel-mark \
--with-sigaltstack=no \
--with-tls=pthread \
--with-glib=embedded \
--enable-nls=no \
mono_cv_uscore=yes"

function clean_build
{
	make clean && make distclean
	rm android_cross.cache

	pushd eglib
	autoreconf -i
	popd
	autoreconf -i

	./configure $CONFIG_OPTS \
	PATH="$PATH" CC="$CC" CXX="$CXX" CPP="$CPP" CXXCPP="$CXXCPP" \
	CFLAGS="$CFLAGS $1" CXXFLAGS="$CXXFLAGS $1" LDFLAGS="$LDFLAGS $2" \
	LD=$LD AR=$AR AS=$AS RANLIB=$RANLIB STRIP=$STRIP CPATH="$CPATH"

	if [ "$?" -ne "0" ]; then 
		echo "Configure FAILED!"
		exit 1
	fi

	make && echo "Build SUCCESS!" || exit 1

	mkdir -p $3
	cp mono/mini/.libs/libmono.a $3
	cp mono/mini/.libs/libmono.so $3
}

if [ x$1 != x"dontclean" ]; then
	rm -rf $OUTDIR
fi

clean_build "" "" "$OUTDIR/x86"

if [ x$1 != x"dontclean" ]; then
NUM_LIBS_BUILT=`ls -AlR $OUTDIR | grep libmono | wc -l`
if [ $NUM_LIBS_BUILT -eq 2 ]; then
	echo "Android STATIC/SHARED libraries are found here: $OUTDIR"
else
	echo "Build failed? Android STATIC/SHARED library cannot be found... Found $NUM_LIBS_BUILT libs under $OUTDIR"
	ls -AlR $OUTDIR
	exit 1
fi
fi
