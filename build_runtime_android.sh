#!/bin/sh

# NB! Prereq : ANDROID_NDK_ROOT=/usr/local/android-ndk-xxx or similar
# Todo: set appropriate ARM flags for hard floats

ANDROID_PLATFORM=android-5
GCC_VERSION=4.4.0
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

PLATFORM_ROOT=$NDK_ROOT/build/platforms/$ANDROID_PLATFORM/arch-arm
TOOLCHAIN=$NDK_ROOT/build/prebuilt/$HOST_ENV/arm-eabi-$GCC_VERSION

if [ ! -a $TOOLCHAIN -o ! -a $PLATFORM_ROOT ]; then
	NDK_NAME=`basename $NDK_ROOT`
	echo "Failed to locate toolchain/platform; $NDK_NAME | $HOST_ENV | $GCC_VERSION | $ANDROID_PLATFORM"
	exit 1
fi

PATH="$TOOLCHAIN/bin:$PATH"
CC="$TOOLCHAIN/bin/arm-eabi-gcc -nostdlib"
CXX="$TOOLCHAIN/bin/arm-eabi-g++ -nostdlib"
CPP="$TOOLCHAIN/bin/arm-eabi-cpp"
CXXCPP="$TOOLCHAIN/bin/arm-eabi-cpp"
CPATH="$PLATFORM_ROOT/usr/include"
LD=$TOOLCHAIN/bin/arm-eabi-ld
AS=$TOOLCHAIN/bin/arm-eabi-as
AR=$TOOLCHAIN/bin/arm-eabi-ar
RANLIB=$TOOLCHAIN/bin/arm-eabi-ranlib
STRIP=$TOOLCHAIN/bin/arm-eabi-strip
CFLAGS="\
-DANDROID -DPLATFORM_ANDROID -DLINUX -D__linux__ \
-DHAVE_USR_INCLUDE_MALLOC_H -DPAGE_SIZE=0x1000 \
-D_POSIX_PATH_MAX=256 -DS_IWRITE=S_IWUSR \
-DHAVE_PTHREAD_MUTEX_TIMEDLOCK \
-fpic -g -I$PLATFORM_ROOT/usr/include \
-fvisibility=hidden -ffunction-sections -fdata-sections"
CXXFLAGS=$CFLAGS
LDFLAGS="\
-Wl,--no-undefined \
-Wl,-T,$TOOLCHAIN/arm-eabi/lib/ldscripts/armelf.x \
-L$PLATFORM_ROOT/usr/lib \
-Wl,-rpath-link=$PLATFORM_ROOT/usr/lib \
-ldl -lm -llog -lc"

CONFIG_OPTS="\
--prefix=$PREFIX \
--cache-file=android_cross.cache \
--host=arm-eabi-linux \
--disable-mcs-build \
--disable-parallel-mark \
--enable-shared=no \
--with-sigaltstack=no \
--with-tls=pthread \
--with-glib=embedded \
--enable-nls=no \
mono_cv_uscore=yes"

if [ ${UNITY_THISISABUILDMACHINE:+1} ]; then
        echo "Erasing builds folder to make sure we start with a clean slate"
        rm -rf builds
fi

function clean_build
{
	pushd mono

	make clean && make distclean
	rm android_cross.cache

	pushd eglib
	autoreconf -i
	popd
	autoreconf -i

	./configure $CONFIG_OPTS \
	PATH="$PATH" CC="$CC" CXX="$CXX" CPP="$CPP" CXXCPP="$CXXCPP" \
	CFLAGS="$CFLAGS $1" CXXFLAGS="$CXXFLAGS" LDFLAGS="$LDFLAGS" \
	LD=$LD AR=$AR AS=$AS RANLIB=$RANLIB STRIP=$STRIP CPATH="$CPATH"

	if [ "$?" -ne "0" ]; then 
		echo "Configure FAILED!"
		exit 1
	fi

	make && echo "Build SUCCESS!" || exit 1

	mkdir -p `dirname ../$2`
	cp mono/mini/.libs/libmono.a ../$2

	popd
}

clean_build -DARM_FPU_NONE=1 $OUTDIR/libmono_fpu_none.a
clean_build -DARM_FPU_VFP=1 $OUTDIR/libmono_fpu_vfp.a

NUM_LIBS_BUILT=`ls -Al $OUTDIR | grep libmono | wc -l`
if [ $NUM_LIBS_BUILT -eq 2 ]; then
	echo "Android STATIC libraries are found here: $OUTDIR"
else
	echo "Build failed? Android STATIC library cannot be found... Found $NUM_LIBS_BUILT libs under $OUTDIR"
	ls -Al $OUTDIR
	exit 1
fi
