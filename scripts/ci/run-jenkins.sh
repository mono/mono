#!/bin/bash -e

export MONO_REPO_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )/../../" && pwd )"
export TESTCMD=${MONO_REPO_ROOT}/scripts/ci/run-step.sh

export CI_CPU_COUNT=$(getconf _NPROCESSORS_ONLN || echo 4)

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
    export EXTRA_CFLAGS="$EXTRA_CFLAGS -ggdb3 -O2"
fi

if [[ $CI_TAGS == *'collect-coverage'* ]]; then
    # Collect coverage for further use by lcov and similar tools.
    # Coverage must be collected with -O0 and debug information.
    export CFLAGS="$CFLAGS -ggdb3 --coverage -O0"
fi

if [[ $CI_TAGS == *'retry-flaky-tests'* ]]; then
    export MONO_FLAKY_TEST_RETRIES=5
fi

# We don't want to have to maintain symbolification blobs for CI
export EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-crash-privacy=no "

if [[ ${CI_TAGS} == *'osx-i386'* ]]; then EXTRA_CFLAGS="$EXTRA_CFLAGS -m32 -arch i386 -mmacosx-version-min=10.9"; EXTRA_LDFLAGS="$EXTRA_LDFLAGS -m32 -arch i386"; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib --host=i386-apple-darwin13.0.0 --build=i386-apple-darwin13.0.0"; fi
if [[ ${CI_TAGS} == *'osx-amd64'* ]]; then EXTRA_CFLAGS="$EXTRA_CFLAGS -m64 -arch x86_64 -mmacosx-version-min=10.9"; EXTRA_LDFLAGS="$EXTRA_LDFLAGS -m64 -arch x86_64" EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib"; fi
if [[ ${CI_TAGS} == *'win-i386'* ]]; then PLATFORM=Win32; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=i686-w64-mingw32"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/Win32/bin/Release/mono-sgen.exe"; fi
if [[ ${CI_TAGS} == *'win-amd64'* ]]; then PLATFORM=x64; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=x86_64-w64-mingw32 --disable-boehm"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/x64/bin/Release/mono-sgen.exe"; fi

if   [[ ${CI_TAGS} == *'coop-suspend'* ]];   then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --disable-hybrid-suspend --enable-cooperative-suspend";
elif [[ ${CI_TAGS} == *'hybrid-suspend'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-hybrid-suspend";
elif [[ ${CI_TAGS} == *'preemptive-suspend'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --disable-hybrid-suspend";
fi

if [[ ${CI_TAGS} == *'checked-coop'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-checked-build=gc,thread"; fi
if [[ ${CI_TAGS} == *'checked-all'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-checked-build=all"; fi

if [[ ${CI_TAGS} == *'mcs-compiler'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-csc=mcs"; fi
if [[ ${CI_TAGS} == *'disable-mcs-build'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --disable-mcs-build"; fi

if   [[ ${CI_TAGS} == *'fullaot_llvm'* ]];       then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=fullaot_llvm ";
elif [[ ${CI_TAGS} == *'hybridaot_llvm'* ]];     then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=hybridaot_llvm";
elif [[ ${CI_TAGS} == *'aot_llvm'* ]];           then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=aot_llvm ";
elif [[ ${CI_TAGS} == *'jit_llvm'* ]];           then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes"; export MONO_ENV_OPTIONS="$MONO_ENV_OPTIONS --llvm";
elif [[ ${CI_TAGS} == *'fullaotinterp'* ]];      then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=fullaotinterp";
elif [[ ${CI_TAGS} == *'fullaot'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=fullaot";
elif [[ ${CI_TAGS} == *'hybridaot'* ]];          then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=hybridaot";
elif [[ ${CI_TAGS} == *'winaot'* ]];             then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=winaot";
elif [[ ${CI_TAGS} == *'aot'* ]];                then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=aot";
elif [[ ${CI_TAGS} == *'bitcode'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=bitcode"; export MONO_ENV_OPTIONS="$MONO_ENV_OPTIONS --aot=clangxx=clang++-6.0";
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

if [[ ${CI_TAGS} == *'cxx'* ]]; then
	EXTRA_CONF_FLAGS="$EXTRA_CONF_FLAGS -enable-cxx"
	MSBUILD_CXX="/p:MONO_COMPILE_AS_CPP=true"
fi

if [[ ${CI_TAGS} == *'win-'* ]];
then
	mkdir -p ~/.config/.mono/
	wget -qO- https://download.mono-project.com/test/new-certs.tgz| tar zx -C ~/.config/.mono/
fi

if [[ ${CI_TAGS} == *'sdks-llvm'* ]]; then
	${TESTCMD} --label=archive --timeout=120m --fatal make -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-llvm-llvm{,win}{32,64} NINJA=
	if [[ ${CI_TAGS} == *'osx-amd64'* ]]; then
		${TESTCMD} --label=archive-llvm36 --timeout=60m --fatal make -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-llvm36-llvm32 NINJA=
	fi
	exit 0
fi

if [[ ${CI_TAGS} == *'sdks-ios'* ]];
   then
	   echo "DISABLE_ANDROID=1" > sdks/Make.config
	   echo "DISABLE_WASM=1" >> sdks/Make.config
	   echo "DISABLE_DESKTOP=1" >> sdks/Make.config
	   if [[ ${CI_TAGS} == *'cxx'* ]]; then
		   echo "ENABLE_CXX=1" >> sdks/Make.config
	   fi
	   export device_test_suites="Mono.Runtime.Tests System.Core"

	   ${TESTCMD} --label=archive --timeout=180m --fatal make -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-ios NINJA=

        if [[ ${CI_TAGS} != *'no-tests'* ]]; then
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
            ${TESTCMD} --label=build-ios-dev-interp-only --timeout=60m make -C sdks/ios build-ios-dev-interp-only-all
            if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
                for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-interp-only-${suite} --timeout=10m make -C sdks/ios run-ios-dev-${suite}; done
            fi
            ${TESTCMD} --label=build-ios-dev-interp-mixed --timeout=60m make -C sdks/ios build-ios-dev-interp-mixed-all
            if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
                for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-interp-mixed-${suite} --timeout=10m make -C sdks/ios run-ios-dev-${suite}; done
            fi
        fi
	   exit 0
fi

if [[ ${CI_TAGS} == *'sdks-android'* ]];
   then
        echo "DISABLE_IOS=1" > sdks/Make.config
        echo "DISABLE_WASM=1" >> sdks/Make.config
        echo "DISABLE_DESKTOP=1" >> sdks/Make.config
        echo "DISABLE_CCACHE=1" >> sdks/Make.config
        if [[ ${CI_TAGS} == *'cxx'* ]]; then
            echo "ENABLE_CXX=1" >> sdks/Make.config
        fi

        # For some very strange reasons, `make -C sdks/android accept-android-license` get stuck when invoked through ${TESTCMD}
        # but doesn't get stuck when called via the shell, so let's just call it here now.
        ${TESTCMD} --label=provision-android --timeout=120m --fatal make -j ${CI_CPU_COUNT} -C sdks/builds provision-android && make -C sdks/android accept-android-license
        ${TESTCMD} --label=provision-mxe --timeout=240m --fatal make -j ${CI_CPU_COUNT} -C sdks/builds provision-mxe
        if [[ ${CI_TAGS} != *'debug'* ]]; then
            ${TESTCMD} --label=archive --timeout=180m --fatal make -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-android NINJA= IGNORE_PROVISION_ANDROID=1 IGNORE_PROVISION_MXE=1
        else
            ${TESTCMD} --label=archive --timeout=180m --fatal make -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-android NINJA= IGNORE_PROVISION_ANDROID=1 IGNORE_PROVISION_MXE=1 CONFIGURATION=debug
        fi

        if [[ ${CI_TAGS} != *'no-tests'* ]]; then
            ${TESTCMD} --label=mini --timeout=60m make -C sdks/android check-mini
            ${TESTCMD} --label=corlib --timeout=60m make -C sdks/android check-corlib
            ${TESTCMD} --label=System --timeout=60m make -C sdks/android check-System
            ${TESTCMD} --label=System.Core --timeout=60m make -C sdks/android check-System.Core
            ${TESTCMD} --label=System.Data --timeout=60m make -C sdks/android check-System.Data
            ${TESTCMD} --label=System.IO.Compression.FileSystem --timeout=60m make -C sdks/android check-System.IO.Compression.FileSystem
            ${TESTCMD} --label=System.IO.Compression --timeout=60m make -C sdks/android check-System.IO.Compression
            ${TESTCMD} --label=System.Json --timeout=60m make -C sdks/android check-System.Json
            ${TESTCMD} --label=System.Net.Http --timeout=60m make -C sdks/android check-System.Net.Http
            ${TESTCMD} --label=System.Numerics --timeout=60m make -C sdks/android check-System.Numerics
            ${TESTCMD} --label=System.Runtime.Serialization --timeout=60m make -C sdks/android check-System.Runtime.Serialization
            ${TESTCMD} --label=System.ServiceModel.Web --timeout=60m make -C sdks/android check-System.ServiceModel.Web
            ${TESTCMD} --label=System.Transactions --timeout=60m make -C sdks/android check-System.Transactions
            ${TESTCMD} --label=System.Xml --timeout=60m make -C sdks/android check-System.Xml
            ${TESTCMD} --label=System.Xml.Linq --timeout=60m make -C sdks/android check-System.Xml.Linq
            ${TESTCMD} --label=Mono.CSharp --timeout=60m make -C sdks/android check-Mono.CSharp
            ${TESTCMD} --label=Mono.Data.Sqlite --timeout=60m make -C sdks/android check-Mono.Data.Sqlite
            ${TESTCMD} --label=Mono.Data.Tds --timeout=60m make -C sdks/android check-Mono.Data.Tds
            ${TESTCMD} --label=Mono.Security --timeout=60m make -C sdks/android check-Mono.Security
        fi
        exit 0
fi

if [[ ${CI_TAGS} == *'webassembly'* ]] || [[ ${CI_TAGS} == *'wasm'* ]];
   then
	   echo "DISABLE_ANDROID=1" > sdks/Make.config
	   echo "DISABLE_IOS=1" >> sdks/Make.config
	   echo "DISABLE_DESKTOP=1" >> sdks/Make.config
	   if [[ ${CI_TAGS} == *'cxx'* ]]; then
	       echo "ENABLE_CXX=1" >> sdks/Make.config
	   fi

	   export aot_test_suites="System.Core"

	   ${TESTCMD} --label=provision --timeout=20m --fatal make --output-sync=recurse --trace -C sdks/builds provision-wasm
	   ${TESTCMD} --label=archive --timeout=180m --fatal make -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-wasm NINJA=

        if [[ ${CI_TAGS} != *'no-tests'* ]]; then
            ${TESTCMD} --label=wasm-build --timeout=60m --fatal make -j ${CI_CPU_COUNT} -C sdks/wasm build
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
            ${TESTCMD} --label=aot-mini --timeout=60m make -j ${CI_CPU_COUNT} -C sdks/wasm run-aot-mini
            ${TESTCMD} --label=build-aot-all --timeout=60m make -j ${CI_CPU_COUNT} -C sdks/wasm build-aot-all
            for suite in ${aot_test_suites}; do ${TESTCMD} --label=run-aot-${suite} --timeout=10m make -C sdks/wasm run-aot-${suite}; done
            #${TESTCMD} --label=check-aot --timeout=60m make -C sdks/wasm check-aot
            ${TESTCMD} --label=package --timeout=60m make -C sdks/wasm package
        fi
	   exit 0
fi


if [[ ${CI_TAGS} != *'mac-sdk'* ]]; # Mac SDK builds Mono itself
	then
	echo ./autogen.sh CFLAGS="$CFLAGS $EXTRA_CFLAGS" CXXFLAGS="$CXXFLAGS $EXTRA_CXXFLAGS" LDFLAGS="$LDFLAGS $EXTRA_LDFLAGS" $EXTRA_CONF_FLAGS
	${TESTCMD} --label=configure --timeout=60m --fatal ./autogen.sh CFLAGS="$CFLAGS $EXTRA_CFLAGS" CXXFLAGS="$CXXFLAGS $EXTRA_CXXFLAGS" LDFLAGS="$LDFLAGS $EXTRA_LDFLAGS" $EXTRA_CONF_FLAGS
fi
if [[ ${CI_TAGS} == *'win-i386'* ]];
    then
	# only build boehm on w32 (only windows platform supporting boehm).
    ${TESTCMD} --label=make-msvc --timeout=60m --fatal /cygdrive/c/Program\ Files\ \(x86\)/MSBuild/14.0/Bin/MSBuild.exe /p:PlatformToolset=v140 /p:Platform=${PLATFORM} /p:Configuration=Release ${MSBUILD_CXX} /p:MONO_TARGET_GC=boehm msvc/mono.sln
fi
if [[ ${CI_TAGS} == *'win-'* ]];
    then
    ${TESTCMD} --label=make-msvc-sgen --timeout=60m --fatal /cygdrive/c/Program\ Files\ \(x86\)/MSBuild/14.0/Bin/MSBuild.exe /p:PlatformToolset=v140 /p:Platform=${PLATFORM} /p:Configuration=Release ${MSBUILD_CXX} /p:MONO_TARGET_GC=sgen msvc/mono.sln
fi

if [[ ${CI_TAGS} == *'winaot'* ]];
    then
    # The AOT compiler on Windows requires Visual Studio's clang.exe and link.exe in $PATH
    # and we must make sure Visual Studio's link.exe comes before Cygwin's link.exe
    VC_ROOT="/cygdrive/c/Program Files (x86)/Microsoft Visual Studio 14.0/VC"
    export PATH="$VC_ROOT/ClangC2/bin/amd64:$VC_ROOT/bin/amd64":$PATH
fi

if [[ ${CI_TAGS} == *'monolite'* ]]; then make get-monolite-latest; fi

make_parallelism="-j ${CI_CPU_COUNT}"
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

${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
