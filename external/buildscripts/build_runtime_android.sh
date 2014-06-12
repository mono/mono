#!/bin/bash

# NB! Prereq : ANDROID_NDK_ROOT=/usr/local/android-ndk-xxx or similar
# Todo: set appropriate ARM flags for hard floats

export ANDROID_PLATFORM=android-5
GCC_PREFIX=arm-linux-androideabi-
GCC_VERSION=4.4.3
OUTDIR=builds/embedruntimes/android
CWD="$(pwd)"
PREFIX="$CWD/builds/android"
BUILDSCRIPTSDIR=external/buildscripts

perl ${BUILDSCRIPTSDIR}/PrepareAndroidSDK.pl -ndk=r8e -env=envsetup.sh && source envsetup.sh

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

PLATFORM_ROOT=$NDK_ROOT/platforms/$ANDROID_PLATFORM/arch-arm
TOOLCHAIN=$NDK_ROOT/toolchains/$GCC_PREFIX$GCC_VERSION/prebuilt/$HOST_ENV

if [ ! -a $TOOLCHAIN -o ! -a $PLATFORM_ROOT ]; then
	NDK_NAME=`basename $NDK_ROOT`
	echo "Failed to locate toolchain/platform; $NDK_NAME | $HOST_ENV | $GCC_PREFIX$GCC_VERSION | $ANDROID_PLATFORM"
	exit 1
fi

KRAIT_PATCH_PATH="${CWD}/external/android_krait_signal_handler"
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
-fpic -g -funwind-tables \
-ffunction-sections -fdata-sections"
CXXFLAGS=$CFLAGS
LDFLAGS="\
-Wl,--wrap,sigaction \
-L${KRAIT_PATCH_PATH}/obj/local/armeabi -lkrait-signal-handler \
-Wl,--no-undefined \
-Wl,-rpath-link=$PLATFORM_ROOT/usr/lib \
-ldl -lm -llog -lc"

CONFIG_OPTS="\
--prefix=$PREFIX \
--cache-file=android_cross.cache \
--host=arm-eabi-linux \
--disable-mcs-build \
--disable-parallel-mark \
--disable-shared-handles \
--with-sigaltstack=no \
--with-tls=pthread \
--with-glib=embedded \
--enable-nls=no \
mono_cv_uscore=yes"

if [ ${UNITY_THISISABUILDMACHINE:+1} ]; then
        echo "Erasing builds folder to make sure we start with a clean slate"
        rm -rf builds
fi

function clean_build_krait_patch
{
       local KRAIT_PATCH_REPO="git://github.com/Unity-Technologies/krait-signal-handler.git"
       if [ ${UNITY_THISISABUILDMACHINE:+1} ]; then
               echo "Trusting TC to have cloned krait patch repository for us"
       elif [ -d "$KRAIT_PATCH_PATH" ]; then
               echo "Krait patch repository already cloned"
       else
               git clone --branch "master" "$KRAIT_PATCH_REPO" "$KRAIT_PATCH_PATH"
       fi
       (cd "$KRAIT_PATCH_PATH" && ./build.pl)
}

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

CCFLAGS_ARMv5_CPU="-DARM_FPU_NONE=1 -march=armv5te -mtune=xscale -msoft-float"
CCFLAGS_ARMv6_VFP="-DARM_FPU_VFP=1  -march=armv6 -mtune=xscale -msoft-float -mfloat-abi=softfp -mfpu=vfp -DHAVE_ARMV6=1"
CCFLAGS_ARMv7_VFP="-DARM_FPU_VFP=1  -march=armv7-a                            -mfloat-abi=softfp -mfpu=vfp -DHAVE_ARMV6=1"
LDFLAGS_ARMv5=""
LDFLAGS_ARMv7="-Wl,--fix-cortex-a8"

rm -rf $OUTDIR

clean_build_krait_patch

clean_build "$CCFLAGS_ARMv5_CPU" "$LDFLAGS_ARMv5" "$OUTDIR/armv5"
clean_build "$CCFLAGS_ARMv6_VFP" "$LDFLAGS_ARMv5" "$OUTDIR/armv6_vfp"
clean_build "$CCFLAGS_ARMv7_VFP" "$LDFLAGS_ARMv7" "$OUTDIR/armv7a"

# works only with ndk-r6b and later
source ${BUILDSCRIPTSDIR}/build_runtime_android_x86.sh dontclean

NUM_LIBS_BUILT=`ls -AlR $OUTDIR | grep libmono | wc -l`
if [ $NUM_LIBS_BUILT -eq 8 ]; then
	echo "Android STATIC/SHARED libraries are found here: $OUTDIR"
else
	echo "Build failed? Android STATIC/SHARED library cannot be found... Found $NUM_LIBS_BUILT libs under $OUTDIR"
	ls -Al $OUTDIR
	exit 1
fi
