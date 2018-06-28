#!/bin/bash -e

export MONO_REPO_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )/../../" && pwd )"
export TESTCMD=${MONO_REPO_ROOT}/scripts/ci/run-step.sh

export TEST_HARNESS_VERBOSE=1

make_timeout=300m

if [[ ${CI_TAGS} == *'clang-sanitizer'* ]]; then
	export CC="clang"
	export CXX="clang++"
	export CFLAGS="$CFLAGS -g -O1 -fsanitize=thread -fsanitize-blacklist=${MONO_REPO_ROOT}/scripts/ci/clang-thread-sanitizer-blacklist -mllvm -tsan-instrument-atomics=false"
	export LDFLAGS="-fsanitize=thread"
	# TSAN_OPTIONS are used by programs that were compiled with Clang's ThreadSanitizer
	# see https://github.com/google/sanitizers/wiki/ThreadSanitizerFlags for more details
	export TSAN_OPTIONS="history_size=7:exitcode=0:force_seq_cst_atomics=1"
	make_timeout=30m
fi

if [[ ${CI_TAGS} == *'win-'* ]]; then
    # Passing -ggdb3 on Cygwin breaks linking against libmonosgen-x.y.dll
    export CFLAGS="$CFLAGS -g -O2"
else
    export CFLAGS="$CFLAGS -ggdb3 -O2"
fi

if [[ $CI_TAGS == *'collect-coverage'* ]]; then
    # Collect coverage for further use by lcov and similar tools.
    # Coverage must be collected with -O0 and debug information.
    export CFLAGS="$CFLAGS -ggdb3 --coverage -O0"
fi

if [[ $CI_TAGS == *'retry-flaky-tests'* ]]; then
    export MONO_FLAKY_TEST_RETRIES=5
fi

if [[ ${CI_TAGS} == *'osx-i386'* ]]; then CFLAGS="$CFLAGS -m32 -arch i386"; LDFLAGS="$LDFLAGS -m32 -arch i386"; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib --host=i386-apple-darwin11.2.0 --build=i386-apple-darwin11.2.0"; fi
if [[ ${CI_TAGS} == *'osx-amd64'* ]]; then CFLAGS="$CFLAGS -m64 -arch x86_64"; LDFLAGS="$LDFLAGS -m64 -arch x86_64" EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib"; fi
if [[ ${CI_TAGS} == *'win-i386'* ]]; then PLATFORM=Win32; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=i686-w64-mingw32"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/Win32/bin/Release/mono-sgen.exe"; fi
if [[ ${CI_TAGS} == *'win-amd64'* ]]; then PLATFORM=x64; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=x86_64-w64-mingw32 --disable-boehm"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/x64/bin/Release/mono-sgen.exe"; fi

if   [[ ${CI_TAGS} == *'coop-suspend'* ]];   then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-cooperative-suspend";
elif [[ ${CI_TAGS} == *'hybrid-suspend'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-hybrid-suspend";
fi

if [[ ${CI_TAGS} == *'checked-coop'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-checked-build=gc,thread"; fi
if [[ ${CI_TAGS} == *'checked-all'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-checked-build=all"; fi

if [[ ${CI_TAGS} == *'mcs-compiler'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-csc=mcs"; fi
if [[ ${CI_TAGS} == *'disable-mcs-build'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --disable-mcs-build"; fi

if   [[ ${CI_TAGS} == *'fullaot_llvm'* ]];       then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=fullaot_llvm ";
elif [[ ${CI_TAGS} == *'hybridaot_llvm'* ]];     then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=hybridaot_llvm";
elif [[ ${CI_TAGS} == *'aot_llvm'* ]];           then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=aot_llvm ";
elif [[ ${CI_TAGS} == *'jit_llvm'* ]];           then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes"; export MONO_ENV_OPTIONS="$MONO_ENV_OPTIONS --llvm";
elif [[ ${CI_TAGS} == *'fullaot'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=fullaot";
elif [[ ${CI_TAGS} == *'hybridaot'* ]];          then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=hybridaot";
elif [[ ${CI_TAGS} == *'winaot'* ]];             then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=winaot";
elif [[ ${CI_TAGS} == *'aot'* ]];                then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=aot";
elif [[ ${CI_TAGS} == *'bitcode'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=bitcode";
elif [[ ${CI_TAGS} == *'acceptance-tests'* ]];   then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --prefix=${MONO_REPO_ROOT}/tmp/mono-acceptance-tests --with-sgen-default-concurrent=yes";
elif [[ ${CI_TAGS} == *'all-profiles'* ]]; then
    # only enable build of the additional profiles on one config to save time
    EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=all"
    # when building profiles like monotouch/monodroid which don't build System.Drawing.dll in the Mono repo we need
    # to build the facades against _something_ to satisfy the typeforwards. In CI we can cheat a little and pass
    # them System.Drawing.dll from the 'build' profile since we don't test those profiles here (we just ensure they compile).
    export EXTERNAL_FACADE_DRAWING_REFERENCE=${MONO_REPO_ROOT}/mcs/class/lib/build/System.Drawing.dll
fi

if [ -x "/usr/bin/dpkg-architecture" ];
	then
	EXTRA_CONF_FLAGS="$EXTRA_CONF_FLAGS --host=`/usr/bin/dpkg-architecture -qDEB_HOST_GNU_TYPE`"
	#force build arch = dpkg arch, sometimes misdetected
	mkdir -p ~/.config/.mono/
	wget -qO- https://download.mono-project.com/test/new-certs.tgz| tar zx -C ~/.config/.mono/
fi

if [[ ${CI_TAGS} == *'win-'* ]];
then
	mkdir -p ~/.config/.mono/
	wget -qO- https://download.mono-project.com/test/new-certs.tgz| tar zx -C ~/.config/.mono/
fi

if [[ ${CI_TAGS} == *'product-sdks-ios'* ]];
   then
	   echo "DISABLE_ANDROID=1" > sdks/Make.config
	   echo "DISABLE_WASM=1" >> sdks/Make.config
	   export device_test_suites="Mono.Runtime.Tests System.Core"
	   ${TESTCMD} --label=runtimes --timeout=60m --fatal make -j4 -C sdks/builds package-ios-sim64 package-ios-target64
	   ${TESTCMD} --label=cross --timeout=60m --fatal make -j4 -C sdks/builds package-ios-cross64 package-ios-cross32
	   ${TESTCMD} --label=bcl --timeout=60m --fatal make -j4 -C sdks/builds package-bcl
	   ${TESTCMD} --label=build-tests --timeout=10m --fatal make -C sdks/ios compile-tests
	   ${TESTCMD} --label=run-sim --timeout=20m make -C sdks/ios run-ios-sim-all
	   ${TESTCMD} --label=build-ios-dev --timeout=60m make -C sdks/ios build-ios-dev-all
	   if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
		   for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-${suite} --timeout=10m make -C sdks/ios run-ios-dev-${suite}; done
	   fi
	   ${TESTCMD} --label=build-ios-dev-llvm --timeout=60m make -C sdks/ios build-ios-dev-llvm-all
	   if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
		   for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-llvm-${suite} --timeout=10m make -C sdks/ios run-ios-dev-${suite}; done
	   fi
	   ${TESTCMD} --label=package --timeout=60m tar cvzf mono-product-sdk-$GIT_COMMIT.tar.gz -C sdks/out/ bcl ios-llvm64 ios-llvm32 ios-cross32-release ios-cross64-release
	   exit 0
fi

if [[ ${CI_TAGS} == *'product-sdks-android'* ]];
   then
        echo "IGNORE_PROVISION_ANDROID=1" > sdks/Make.config
        echo "IGNORE_PROVISION_MXE=1" >> sdks/Make.config
        echo "DISABLE_CCACHE=1" >> sdks/Make.config
        ${TESTCMD} --label=provision-android --timeout=120m --fatal make -j4 -C sdks/builds provision-android
        if [[ ${CI_TAGS} == *'provision-mxe'* ]]; then
            ${TESTCMD} --label=provision-mxe --timeout=240m --fatal make -j4 -C sdks/builds provision-mxe
        fi
        ${TESTCMD} --label=runtimes --timeout=120m --fatal make -j4 -C sdks/builds package-android-{armeabi,armeabi-v7a,arm64-v8a,x86,x86_64} package-android-host-{Darwin,mxe-Win64}
        exit 0
fi

if [[ ${CI_TAGS} == *'webassembly'* ]];
   then
	   echo "DISABLE_ANDROID=1" > sdks/Make.config
	   echo "DISABLE_IOS=1" >> sdks/Make.config
	   ${TESTCMD} --label=runtimes --timeout=60m --fatal make -j4 -C sdks/builds package-wasm-interp
	   ${TESTCMD} --label=bcl --timeout=60m --fatal make -j4 -C sdks/builds package-bcl
	   ${TESTCMD} --label=wasm-build --timeout=60m --fatal make -j4 -C sdks/wasm build
	   ${TESTCMD} --label=ch-mini-test --timeout=60m make -C sdks/wasm run-ch-mini
	   ${TESTCMD} --label=v8-mini-test --timeout=60m make -C sdks/wasm run-v8-mini
	   ${TESTCMD} --label=sm-mini-test --timeout=60m make -C sdks/wasm run-sm-mini
	   ${TESTCMD} --label=jsc-mini-test --timeout=60m make -C sdks/wasm run-jsc-mini
	   #The following tests are not passing yet, so enabling them would make us perma-red
	   #${TESTCMD} --label=mini-corlib --timeout=60m make -C sdks/wasm run-all-corlib
	   #${TESTCMD} --label=mini-system --timeout=60m make -C sdks/wasm run-all-system
	   # Chakra crashes with System.Core. See https://github.com/mono/mono/issues/8345
	   ${TESTCMD} --label=ch-system-core --timeout=60m make -C sdks/wasm run-ch-system-core
	   ${TESTCMD} --label=v8-system-core --timeout=60m make -C sdks/wasm run-v8-system-core
	   ${TESTCMD} --label=sm-system-core --timeout=60m make -C sdks/wasm run-sm-system-core
	   ${TESTCMD} --label=jsc-system-core --timeout=60m make -C sdks/wasm run-jsc-system-core
	   ${TESTCMD} --label=package --timeout=60m make -C sdks/wasm package
	   exit 0
fi


if [[ ${CI_TAGS} != *'mac-sdk'* ]]; # Mac SDK builds Mono itself
	then
	${TESTCMD} --label=configure --timeout=60m --fatal ./autogen.sh $EXTRA_CONF_FLAGS
fi
if [[ ${CI_TAGS} == *'win-i386'* ]];
    then
	# only build boehm on w32 (only windows platform supporting boehm).
    ${TESTCMD} --label=make-msvc --timeout=60m --fatal /cygdrive/c/Program\ Files\ \(x86\)/MSBuild/14.0/Bin/MSBuild.exe /p:PlatformToolset=v140 /p:Platform=${PLATFORM} /p:Configuration=Release /p:MONO_TARGET_GC=boehm msvc/mono.sln
fi
if [[ ${CI_TAGS} == *'win-'* ]];
    then
    ${TESTCMD} --label=make-msvc-sgen --timeout=60m --fatal /cygdrive/c/Program\ Files\ \(x86\)/MSBuild/14.0/Bin/MSBuild.exe /p:PlatformToolset=v140 /p:Platform=${PLATFORM} /p:Configuration=Release /p:MONO_TARGET_GC=sgen msvc/mono.sln
fi

if [[ ${CI_TAGS} == *'winaot'* ]];
    then
    # The AOT compiler on Windows requires Visual Studio's clang.exe and link.exe in $PATH
    # and we must make sure Visual Studio's link.exe comes before Cygwin's link.exe
    VC_ROOT="/cygdrive/c/Program Files (x86)/Microsoft Visual Studio 14.0/VC"
    export PATH="$VC_ROOT/ClangC2/bin/amd64:$VC_ROOT/bin/amd64":$PATH
fi

if [[ ${CI_TAGS} == *'monolite'* ]]; then make get-monolite-latest; fi

make_parallelism=-j4
if [[ ${CI_TAGS} == *'linux-ppc64el'* ]]; then make_parallelism=-j1; fi

make_continue=
if [[ ${CI_TAGS} == *'checked-all'* ]]; then make_continue=-k; fi

if [[ ${CI_TAGS} != *'mac-sdk'* ]]; # Mac SDK builds Mono itself
	then
	${TESTCMD} --label=make --timeout=${make_timeout} --fatal make ${make_parallelism} ${make_continue} -w V=1
fi

if [[ ${CI_TAGS} == *'checked-coop'* ]]; then export MONO_CHECK_MODE=gc,thread; fi
if [[ ${CI_TAGS} == *'checked-all'* ]]; then export MONO_CHECK_MODE=all; fi

export MONO_ENV_OPTIONS="$MONO_ENV_OPTIONS $MONO_TEST_ENV_OPTIONS"

if   [[ ${CI_TAGS} == *'acceptance-tests'* ]];         then ${MONO_REPO_ROOT}/scripts/ci/run-test-acceptance-tests.sh;
elif [[ ${CI_TAGS} == *'profiler-stress-tests'* ]];    then ${MONO_REPO_ROOT}/scripts/ci/run-test-profiler-stress-tests.sh;
elif [[ ${CI_TAGS} == *'stress-tests'* ]];             then ${MONO_REPO_ROOT}/scripts/ci/run-test-stress-tests.sh;
elif [[ ${CI_TAGS} == *'interpreter'* ]];              then ${MONO_REPO_ROOT}/scripts/ci/run-test-interpreter.sh;
elif [[ ${CI_TAGS} == *'mcs-compiler'* ]];             then ${MONO_REPO_ROOT}/scripts/ci/run-test-mcs.sh;
elif [[ ${CI_TAGS} == *'mac-sdk'* ]];                  then ${MONO_REPO_ROOT}/scripts/ci/run-test-mac-sdk.sh;
elif [[ ${CI_TAGS} == *'no-tests'* ]];                 then exit 0;
else make check-ci;
fi
