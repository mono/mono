#!/bin/sh
SDK_VERSION=5.0
MAC_SDK_VERSION=10.6
ASPEN_ROOT=/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer
SIMULATOR_ASPEN_ROOT=/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer
XCOMP_ASPEN_ROOT=/Applications/Xcode.app/Contents/Developer/Platforms/MacOSX.platform/Developer/SDKs/MacOSX${MAC_SDK_VERSION}.sdk
BUILDSCRIPTSDIR=external/buildscripts

if [ ! -d $ASPEN_ROOT/SDKs/iPhoneOS${SDK_VERSION}.sdk ]; then
	SDK_VERSION=5.1
fi

echo "Using SDK $SDK_VERSION"

ASPEN_SDK=$ASPEN_ROOT/SDKs/iPhoneOS${SDK_VERSION}.sdk/
SIMULATOR_ASPEN_SDK=$SIMULATOR_ASPEN_ROOT/SDKs/iPhoneSimulator${SDK_VERSION}.sdk

ORIG_PATH=$PATH
PRFX=$PWD/tmp 



if [ ${UNITY_THISISABUILDMACHINE:+1} ]; then
        echo "Erasing builds folder to make sure we start with a clean slate"
        rm -rf builds
fi

setenv () {
	export PATH=$ASPEN_ROOT/usr/bin:$PATH

	export C_INCLUDE_PATH="$ASPEN_SDK/usr/lib/gcc/arm-apple-darwin9/4.2.1/include:$ASPEN_SDK/usr/include"
	export CPLUS_INCLUDE_PATH="$ASPEN_SDK/usr/lib/gcc/arm-apple-darwin9/4.2.1/include:$ASPEN_SDK/usr/include"
	#export CFLAGS="-DZ_PREFIX -DPLATFORM_IPHONE -DARM_FPU_VFP=1 -miphoneos-version-min=3.0 -mno-thumb -fvisibility=hidden -g -O0"
	export CFLAGS="-DHAVE_ARMV6=1 -DZ_PREFIX -DPLATFORM_IPHONE -DARM_FPU_VFP=1 -miphoneos-version-min=3.0 -mno-thumb -fvisibility=hidden -Os"
	export CXXFLAGS="$CFLAGS"
	export CC="gcc -arch $1"
	export CXX="g++ -arch $1"
	export CPP="cpp -nostdinc -U__powerpc__ -U__i386__ -D__arm__"
	export CXXPP="cpp -nostdinc -U__powerpc__ -U__i386__ -D__arm__"
	export LD=$CC
	export LDFLAGS="-liconv -Wl,-syslibroot,$ASPEN_SDK"
}

unsetenv () {
	export PATH=$ORIG_PATH

	unset C_INCLUDE_PATH
	unset CPLUS_INCLUDE_PATH
	unset CC
	unset CXX
	unset CPP
	unset CXXPP
	unset LD
	unset LDFLAGS
	unset PLATFORM_IPHONE_XCOMP
	unset CFLAGS
	unset CXXFLAGS
}

export mono_cv_uscore=yes
export cv_mono_sizeof_sunpath=104
export ac_cv_func_posix_getpwuid_r=yes
export ac_cv_func_backtrace_symbols=no

build_arm_mono ()
{
	setenv "$1"

	if [ $2 -eq 0 ]; then
		make clean
		rm config.h*

		pushd eglib 
		./autogen.sh --host=arm-apple-darwin9 --prefix=$PRFX
		make clean
		popd

		./autogen.sh --prefix=$PRFX --disable-mcs-build --host=arm-apple-darwin9 --disable-shared-handles --with-tls=pthread --with-sigaltstack=no --with-glib=embedded --enable-minimal=jit,profiler,com --disable-nls || exit 1
		perl -pi -e 's/MONO_SIZEOF_SUNPATH 0/MONO_SIZEOF_SUNPATH 104/' config.h
		perl -pi -e 's/#define HAVE_FINITE 1//' config.h
		#perl -pi -e 's/#define HAVE_MMAP 1//' config.h
		perl -pi -e 's/#define HAVE_CURSES_H 1//' config.h
		perl -pi -e 's/#define HAVE_STRNDUP 1//' eglib/config.h
		make
	else
		echo "Skipping autogen.sh for incremental build"
	fi

	make || exit 1

	mkdir -p builds/embedruntimes/iphone
	cp mono/mini/.libs/libmono.a "builds/embedruntimes/iphone/libmono-$1.a" || exit 1
}

build_iphone_runtime () 
{
	echo "Building iPhone runtime"
	export LIBTOOLIZE=`which glibtoolize`
	build_arm_mono "armv7" $1 || exit 1

	cp builds/embedruntimes/iphone/libmono-armv7.a builds/embedruntimes/iphone/libmono.a
	rm builds/embedruntimes/iphone/libmono-armv7.a

	unsetenv
	echo "iPhone runtime build done"
}

build_iphone_crosscompiler ()
{
	echo "Building iPhone cross compiler";
	export CFLAGS="-DARM_FPU_VFP=1 -DUSE_MUNMAP -DPLATFORM_IPHONE_XCOMP"	
	export CC="gcc -arch i386"
	export CXX="g++ -arch i386"
	export CPP="$CC -E"
	export LD=$CC
	export MACSDKOPTIONS="-mmacosx-version-min=$MAC_SDK_VERSION -isysroot $XCOMP_ASPEN_ROOT"

	# iOS build agents have different libtools in different places :-|
	export LIBTOOLIZE=`which glibtoolize`
	if test "x$LIBTOOLIZE" = x; then
		export LIBTOOLIZE=`which libtoolize`
	fi
	export LIBTOOL=`echo $LIBTOOLIZE | sed 's/ize$//'`

	export PLATFORM_IPHONE_XCOMP=1	

    if [ $1 -eq 0 ]; then
		pushd eglib 
		./autogen.sh --prefix=$PRFX || exit 1
		make clean
		popd
	
		./autogen.sh --prefix=$PRFX --with-macversion=$MAC_SDK_VERSION --disable-mcs-build --disable-shared-handles --with-tls=pthread --with-signalstack=no --with-glib=embedded --target=arm-darwin --disable-nls || exit 1
		perl -pi -e 's/#define HAVE_STRNDUP 1//' eglib/config.h
		make clean || exit 1
	else
		echo "Skipping autogen.sh for incremental build"
	fi

	make || exit 1
	mkdir -p builds/crosscompiler/iphone
	cp mono/mini/mono builds/crosscompiler/iphone/mono-xcompiler
	unsetenv
	echo "iPhone cross compiler build done"
}

build_iphone_simulator ()
{
	echo "Building iPhone simulator static lib";
	export MACSYSROOT="-isysroot $SIMULATOR_ASPEN_SDK"
	export MACSDKOPTIONS="-miphoneos-version-min=3.0 $MACSYSROOT"
	export CC="$SIMULATOR_ASPEN_ROOT/usr/bin/gcc -arch i386"
	export CXX="$SIMULATOR_ASPEN_ROOT/usr/bin/g++ -arch i386"
	export LIBTOOLIZE=`which glibtoolize`
	perl ${BUILDSCRIPTSDIR}/build_runtime_osx.pl -iphone_simulator=1 || exit 1
	echo "Copying iPhone simulator static lib to final destination";
	mkdir -p builds/embedruntimes/iphone
	cp mono/mini/.libs/libmono.a builds/embedruntimes/iphone/libmono-i386.a
	unsetenv
}

usage()
{
	echo "available arguments: [--runtime-only|--xcomp-only|--simulator-only]";
}

INCREMENTAL=0


if [ $# -gt 2 ]; then
 	usage
	exit 1
fi
if [ $# -gt 0 ]; then
	if [ $# -eq 2 ]; then
		if [ "x$2" == "x--incremental" ]; then
			INCREMENTAL=1
		fi
	fi

	if [ "x$1" == "x--runtime-only" ]; then
		build_iphone_runtime $INCREMENTAL || exit 1
	elif [ "x$1" == "x--xcomp-only" ]; then
		build_iphone_crosscompiler $INCREMENTAL || exit 1	
	elif [ "x$1" == "x--simulator-only" ]; then
		build_iphone_simulator $INCREMENTAL || exit 1	
	else
		usage
	fi

fi
if [ $# -eq 0 ]; then
	build_iphone_runtime || exit 1
	build_iphone_crosscompiler || exit 1
	build_iphone_simulator || exit 1
fi
