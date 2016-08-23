#!/bin/sh
SDK_VERSION=9.3
MAC_SDK_VERSION=10.6
ASPEN_ROOT=/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneOS.platform/Developer
SIMULATOR_ASPEN_ROOT=/Applications/Xcode.app/Contents/Developer/Platforms/iPhoneSimulator.platform/Developer
XCOMP_ASPEN_ROOT=$PWD/../../mono-build-deps/build/MacBuildEnvironment/builds/MacOSX${MAC_SDK_VERSION}.sdk
BUILDSCRIPTSDIR=external/buildscripts

# allow to build with older SDKs temporarily
if [ ! -d $ASPEN_ROOT/SDKs/iPhoneOS${SDK_VERSION}.sdk ]; then
	SDK_VERSION=7.1
fi
if [ ! -d $ASPEN_ROOT/SDKs/iPhoneOS${SDK_VERSION}.sdk ]; then
	SDK_VERSION=5.1
fi
if [ ! -d $ASPEN_ROOT/SDKs/iPhoneOS${SDK_VERSION}.sdk ]; then
	SDK_VERSION=5.0
fi

echo "Using SDK $SDK_VERSION"

ASPEN_SDK=$ASPEN_ROOT/SDKs/iPhoneOS${SDK_VERSION}.sdk/
SIMULATOR_ASPEN_SDK=$SIMULATOR_ASPEN_ROOT/SDKs/iPhoneSimulator${SDK_VERSION}.sdk

ORIG_PATH=$PATH
PRFX=$PWD/tmp 
MAKE_JOBS=4

perl "external/buildscripts/prepare_osx_build.pl"

PATH=$PWD/../../mono-build-deps/build/built-tools/bin:$PATH
LIBTOOL=$PWD/../../mono-build-deps/build/built-tools/bin/libtool
LIBTOOLIZE=$PWD/../../mono-build-deps/build/built-tools/bin/libtoolize

echo "LIBTOOL = $LIBTOOL"
echo "LIBTOOLIZE = $LIBTOOLIZE"
echo "PATH = $PATH"
echo ""

if [ ${UNITY_THISISABUILDMACHINE:+1} ]; then
	echo "Erasing builds folder to make sure we start with a clean slate"
	rm -rf builds
	#if test -e /usr/local/bin/libtool; then
	#	LIBTOOL=/usr/local/bin/libtool
	#elif test -e /usr/local/bin/glibtool; then
	#	LIBTOOL=/usr/local/bin/glibtool
	#fi
	MAKE_JOBS=""
else
	MAKE_JOBS="-j$MAKE_JOBS"
fi

setenv () {
	export PATH=$ASPEN_ROOT/usr/bin:$PATH

	export C_INCLUDE_PATH="$ASPEN_SDK/usr/lib/gcc/arm-apple-darwin9/4.2.1/include:$ASPEN_SDK/usr/include"
	export CPLUS_INCLUDE_PATH="$ASPEN_SDK/usr/lib/gcc/arm-apple-darwin9/4.2.1/include:$ASPEN_SDK/usr/include"
	#export CFLAGS="-DZ_PREFIX -DPLATFORM_IPHONE -DARM_FPU_VFP=1 -miphoneos-version-min=3.0 -mno-thumb -fvisibility=hidden -g -O0"
	export CFLAGS="-DHAVE_ARMV6=1 -DZ_PREFIX -DPLATFORM_IPHONE -DARM_FPU_VFP=1 -miphoneos-version-min=3.0 -mno-thumb -fvisibility=hidden -Os -isysroot $ASPEN_ROOT"
	export CPPFLAGS="$CFLAGS -U__powerpc__ -U__i386__ -D__arm__"
	export CXXFLAGS="$CFLAGS -U__powerpc__ -U__i386__ -D__arm__"
	export CC="gcc -arch $1"
	export CXX="g++ -arch $1"
	# export CPP="cpp -nostdinc -U__powerpc__ -U__i386__ -D__arm__"
	# export CXXPP="cpp -nostdinc -U__powerpc__ -U__i386__ -D__arm__"
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
	unset CPPFLAGS
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
		rm -f config.h*

		pushd eglib 
		LIBTOOL=$LIBTOOL ./autogen.sh --host=arm-apple-darwin9 --prefix=$PRFX
		make clean
		popd

		LIBTOOL=$LIBTOOL ./autogen.sh --prefix=$PRFX --disable-mcs-build --host=arm-apple-darwin9 --disable-shared-handles --with-tls=pthread --with-sigaltstack=no --with-glib=embedded --enable-minimal=jit,profiler,com --disable-nls || exit 1
		perl -pi -e 's/MONO_SIZEOF_SUNPATH 0/MONO_SIZEOF_SUNPATH 104/' config.h
		perl -pi -e 's/#define HAVE_FINITE 1//' config.h
		#perl -pi -e 's/#define HAVE_MMAP 1//' config.h
		perl -pi -e 's/#define HAVE_CURSES_H 1//' config.h
		perl -pi -e 's/#define HAVE_STRNDUP 1//' eglib/config.h
		make $MAKE_JOBS
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
	export CPPFLAGS="$CFLAGS"
	export CC="gcc -arch i386"
	export CXX="g++ -arch i386"
	export CPP="$CC -E"
	export LD=$CC
	export MACSDKOPTIONS="-mmacosx-version-min=$MAC_SDK_VERSION -isysroot $XCOMP_ASPEN_ROOT"

	export PLATFORM_IPHONE_XCOMP=1	

    if [ $1 -eq 0 ]; then
		pushd eglib 
		LIBTOOL=$LIBTOOL ./autogen.sh --prefix=$PRFX || exit 1
		make clean
		popd
	
		LIBTOOL=$LIBTOOL ./autogen.sh --prefix=$PRFX --with-macversion=$MAC_SDK_VERSION --disable-mcs-build --disable-shared-handles --with-tls=pthread --with-signalstack=no --with-glib=embedded --target=arm-darwin --disable-nls || exit 1
		perl -pi -e 's/#define HAVE_STRNDUP 1//' eglib/config.h
		make clean || exit 1
	else
		echo "Skipping autogen.sh for incremental build"
	fi

	make $MAKE_JOBS || exit 1
	mkdir -p builds/crosscompiler/iphone
	cp mono/mini/mono builds/crosscompiler/iphone/mono-xcompiler
	unsetenv
	echo "iPhone cross compiler build done"
}

get_xcode_version()
{
	echo $(cat /Applications/Xcode.app/Contents/version.plist | grep -A 1 CFBundleShortVersionString | tail -n 1 | sed 's/.*>\([0-9][0-9]*\).*/\1/g')
}

build_iphone_simulator ()
{
	echo "Building iPhone simulator static lib";
	export CFLAGS="-D_XOPEN_SOURCE=1 -DTARGET_IPHONE_SIMULATOR -g -O0";
	export CPPFLAGS="$CFLAGS"
	export MACSYSROOT="-isysroot $SIMULATOR_ASPEN_SDK"

	# Xcode 4.x does not support -mios-simulator-version-min=4.3 in MACSDKOPTIONS
	export MACSDKOPTIONS="$MACSYSROOT $CFLAGS"
	if [ "4" != "$(get_xcode_version)" ]; then
		export MACSDKOPTIONS="$MACSDKOPTIONS -mios-simulator-version-min=4.3"
	fi
	export CC="$SIMULATOR_ASPEN_ROOT/usr/bin/gcc -arch i386"
	export CXX="$SIMULATOR_ASPEN_ROOT/usr/bin/g++ -arch i386"
	export LIBTOOLIZE=`which glibtoolize`
	export CFLAGS="-D_XOPEN_SOURCE=1 -DTARGET_IPHONE_SIMULATOR -g -O0"

	if [ ${UNITY_THISISABUILDMACHINE:+1} ]; then
		export PATH="/usr/local/bin:$PATH"
	fi

	make distclean

	#were going to tell autogen to use a specific cache file, that we purposely remove before starting.
	#that way, autogen is forced to do all its config stuff again, which should make this buildscript
	#more robust if other targetplatforms have been built from this same workincopy
	rm osx.cache

	pushd eglib
	make distclean
	autoreconf -i
	popd

	# From Massi: I was getting failures in install_name_tool about space
	# for the commands being too small, and adding here things like
	# $ENV{LDFLAGS} = '-headerpad_max_install_names' and
	# $ENV{LDFLAGS} = '-headerpad=0x40000' did not help at all (and also
	# adding them to our final gcc invocation to make the bundle).
	# Lucas noticed that I was lacking a Mono prefix, and having a long
	# one would give us space, so here is this silly looong prefix.
	LONG_PREFIX="/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting/scripting"

	./autogen.sh --cache-file=osx.cache --disable-mcs-build --with-glib=embedded --disable-nls \
		--prefix=$LONG_PREFIX || { echo "failing configuring mono"; exit 1; }
	make clean || echo "failed make cleaning"

	perl -pi -e 's/#define HAVE_STRNDUP 1//' eglib/config.h

	make $MAKE_JOBS || { echo "failing running make for mono"; exit 1; }

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
	INCREMENTAL=0
	build_iphone_runtime $INCREMENTAL || exit 1
	build_iphone_crosscompiler $INCREMENTAL || exit 1
	build_iphone_simulator $INCREMENTAL || exit 1
fi