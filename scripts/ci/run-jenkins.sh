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

if [[ ${label} == w* ]]; then
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

if [[ ${label} == 'osx-i386' ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib --build=i386-apple-darwin11.2.0"; fi
if [[ ${label} == 'osx-amd64' ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-libgdiplus=/Library/Frameworks/Mono.framework/Versions/Current/lib/libgdiplus.dylib "; fi
if [[ ${label} == 'w32' ]]; then PLATFORM=Win32; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=i686-w64-mingw32"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/Win32/bin/Release/mono-sgen.exe"; fi
if [[ ${label} == 'w64' ]]; then PLATFORM=x64; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=x86_64-w64-mingw32 --disable-boehm"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/x64/bin/Release/mono-sgen.exe"; fi

if [[ ${CI_TAGS} == *'coop-gc'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-cooperative-gc=yes"; fi

if [[ ${CI_TAGS} == *'checked-coop'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-checked-build=gc,thread"; fi
if [[ ${CI_TAGS} == *'checked-all'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-checked-build=all"; fi

if [[ ${CI_TAGS} == *'mcs-compiler'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-csc=mcs"; fi
if [[ ${CI_TAGS} == *'disable-mcs-build'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --disable-mcs-build"; fi

if   [[ ${CI_TAGS} == *'fullaot_llvm'* ]];       then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime_preset=fullaot ";
elif [[ ${CI_TAGS} == *'hybridaot_llvm'* ]];     then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime_preset=hybridaot";
elif [[ ${CI_TAGS} == *'aot_llvm'* ]];           then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime_preset=aot ";
elif [[ ${CI_TAGS} == *'fullaot'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime_preset=fullaot";
elif [[ ${CI_TAGS} == *'hybridaot'* ]];          then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime_preset=hybridaot";
elif [[ ${CI_TAGS} == *'winaot'* ]];             then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime_preset=winaot";
elif [[ ${CI_TAGS} == *'aot'* ]];                then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime_preset=aot";
elif [[ ${CI_TAGS} == *'bitcode'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime_preset=bitcode";
elif [[ ${CI_TAGS} == *'acceptance-tests'* ]];   then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --prefix=${MONO_REPO_ROOT}/tmp/mono-acceptance-tests --with-sgen-default-concurrent=yes";
elif [[ ${label} != w* ]] && [[ ${label} != 'debian-8-ppc64el' ]] && [[ ${label} != 'centos-s390x' ]] && [[ ${CI_TAGS} != *'monolite'* ]];
    then
    # only enable the concurrent collector by default on main unix archs
    EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-sgen-default-concurrent=yes"

    if [[ ${label} == 'ubuntu-1404-amd64' ]]; then
        # only enable build of the additional profiles on one architecture to save time
        EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime_preset=all"
        # when building profiles like monotouch/monodroid which don't build System.Drawing.dll in the Mono repo we need
        # to build the facades against _something_ to satisfy the typeforwards. In CI we can cheat a little and pass
        # them System.Drawing.dll from the 'build' profile since we don't test those profiles here (we just ensure they compile).
        export EXTERNAL_FACADE_DRAWING_REFERENCE=${MONO_REPO_ROOT}/mcs/class/lib/build/System.Drawing.dll
    fi
fi

if [ -x "/usr/bin/dpkg-architecture" ];
	then
	EXTRA_CONF_FLAGS="$EXTRA_CONF_FLAGS --host=`/usr/bin/dpkg-architecture -qDEB_HOST_GNU_TYPE`"
	#force build arch = dpkg arch, sometimes misdetected
	mkdir -p ~/.config/.mono/
	wget -qO- https://download.mono-project.com/test/new-certs.tgz| tar zx -C ~/.config/.mono/
fi

if [[ ${CI_TAGS} == 'product-sdks' ]];
   then
	   ${TESTCMD} --label=runtimes --timeout=60m --fatal make -j4 -C sdks/builds package-ios-sim64
	   ${TESTCMD} --label=bcl --timeout=60m --fatal make -j4 -C sdks/builds package-bcl
	   ${TESTCMD} --label=build-tests --timeout=60m --fatal make -C sdks/ios compile-tests
	   ${TESTCMD} --label=run-tests --timeout=60m --fatal make -C sdks/ios run-tests
	   exit 0
fi

if [[ ${CI_TAGS} != *'mac-sdk'* ]]; # Mac SDK builds Mono itself
	then
	${TESTCMD} --label=configure --timeout=60m --fatal ./autogen.sh $EXTRA_CONF_FLAGS
fi
if [[ ${label} == 'w32' ]];
    then
	# only build boehm on w32 (only windows platform supporting boehm).
    ${TESTCMD} --label=make-msvc --timeout=60m --fatal /cygdrive/c/Program\ Files\ \(x86\)/MSBuild/14.0/Bin/MSBuild.exe /p:PlatformToolset=v140 /p:Platform=${PLATFORM} /p:Configuration=Release /p:MONO_TARGET_GC=boehm msvc/mono.sln
fi
if [[ ${label} == w* ]];
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
if [[ ${label} == 'debian-8-ppc64el' ]]; then make_parallelism=-j1; fi

if [[ ${CI_TAGS} != *'mac-sdk'* ]]; # Mac SDK builds Mono itself
	then
	${TESTCMD} --label=make --timeout=${make_timeout} --fatal make ${make_parallelism} -w V=1
fi

if [[ ${CI_TAGS} == *'checked-coop'* ]]; then export MONO_CHECK_MODE=gc,thread; fi
if [[ ${CI_TAGS} == *'checked-all'* ]]; then export MONO_CHECK_MODE=all; fi

export MONO_ENV_OPTIONS="$MONO_ENV_OPTIONS $MONO_TEST_ENV_OPTIONS"

if [[ ${CI_TAGS} == *'acceptance-tests'* ]];
    then
	$(dirname "${BASH_SOURCE[0]}")/run-test-acceptance-tests.sh
elif [[ ${CI_TAGS} == *'profiler-stress-tests'* ]];
    then
    $(dirname "${BASH_SOURCE[0]}")/run-test-profiler-stress-tests.sh
elif [[ ${CI_TAGS} == *'stress-tests'* ]];
    then
    $(dirname "${BASH_SOURCE[0]}")/run-test-stress-tests.sh
elif [[ ${CI_TAGS} == *'interpreter'* ]];
    then
    $(dirname "${BASH_SOURCE[0]}")/run-test-interpreter.sh
elif [[ ${CI_TAGS} == *'mcs-compiler'* ]];
    then
    $(dirname "${BASH_SOURCE[0]}")/run-test-mcs.sh
elif [[ ${CI_TAGS} == *'mac-sdk'* ]];
    then
    $(dirname "${BASH_SOURCE[0]}")/run-test-mac-sdk.sh
elif [[ ${CI_TAGS} == *'no-tests'* ]];
    then
	exit 0
else
	make check-ci
fi
