#!/bin/bash

PREFIX=`pwd`/builds/stv
BUILDSCRIPTSDIR=external/buildscripts

perl ${BUILDSCRIPTSDIR}/SDKDownloader.pm --repo_name=stv-sdk --artifacts_folder=artifacts && source artifacts/SDKDownloader/stv-sdk/env.sh

rm -rf builds

for STV_TARGET in ${STV_TARGETS}; do
	echo "BUILDING FOR $STV_TARGET"
	OUTDIR=builds/embedruntimes/stv/$STV_TARGET

	STV_GCC_PREFIX="STV_GCC_PREFIX_${STV_TARGET}"
	STV_GCC_PREFIX="${!STV_GCC_PREFIX}"

	# need to swap flags when building for different platforms.
	CXXFLAGS="-g -DARM_FPU_VFP=1 -DHAVE_ARMV6=1 -D__ARM_EABI__ -DLINUX -D__linux__ -DHAVE_PTHREAD_MUTEX_TIMEDLOCK -march=armv7-a -mfpu=vfp -mfloat-abi=softfp -fpic -funwind-tables -ffunction-sections -fdata-sections";
	CC="${STV_GCC_PREFIX}gcc"
	CXX="${STV_GCC_PREFIX}g++"
	AR="${STV_GCC_PREFIX}ar"
	LD="${STV_GCC_PREFIX}ld"
	LDFLAGS="-Wl,--fix-cortex-a8"

	echo "CXXFLAGS= ${CXXFLAGS}"
	echo "CC= ${CC}"
	echo "CXX= ${CXX}"
	echo "AR= ${AR}"
	echo "LD= ${LD}"
	echo "LDFLAGS= ${LDFLAGS}"

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

	mkdir -p $OUTDIR
	cp -f mono/mini/.libs/libmono.a $OUTDIR

	if [ -d builds/monodistribution ] ; then
		rm -r builds/monodistribution
	fi
done

# Support backwards compatibility with old file location
cp -f builds/embedruntimes/stv/STANDARD_13/libmono.a builds/embedruntimes/stv/libmono.a

