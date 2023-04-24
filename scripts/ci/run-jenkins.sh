#!/bin/bash -e
# -*- mode: shell-script; indent-tabs-mode: nil; -*-

export MONO_REPO_ROOT="$( cd "$( dirname "${BASH_SOURCE[0]}" )/../../" && pwd )"
export TESTCMD=${MONO_REPO_ROOT}/scripts/ci/run-step.sh
export CI=1
export CI_PR=$([[ ${CI_TAGS} == *'pull-request'* ]] && echo 1 || true)
export CI_CPU_COUNT=$(getconf _NPROCESSORS_ONLN || getconf NPROCESSORS_ONLN || echo 4)
export TEST_HARNESS_VERBOSE=1

# workaround for acceptance-tests submodules leaving files behind since Jenkins only does "git clean -xdf" (no second 'f')
# which won't clean untracked .git repos (remove once https://github.com/jenkinsci/git-plugin/pull/449 is available)
for dir in acceptance-tests/external/*; do [ -d "$dir" ] && (cd "$dir" && echo "Cleaning $dir" && git clean -xdff); done

source ${MONO_REPO_ROOT}/scripts/ci/util.sh

# if [[ ${CI_TAGS} == *'pull-request'* ]]; then
# 	# Skip lanes which are not affected by the PR
# 	wget -O pr-contents.diff "${ghprbPullLink}.diff"
# 	grep '^diff' pr-contents.diff > pr-files.txt
# 	echo "Files affected by the PR:"
# 	cat pr-files.txt

# 	# FIXME: Add more
# 	skip=false
# 	skip_step=""
# 	if ! grep -q -v a/mono/mini/mini-ppc pr-files.txt; then
# 		skip_step="PPC"
# 		skip=true
# 	fi
# 	if ! grep -q -v a/scripts/ci/provisioning pr-files.txt; then
# 		skip_step="CI provisioning scripts"
# 		skip=true
# 	fi
# 	if [ $skip = true ]; then
# 		${TESTCMD} --label="Skipped on ${skip_step}." --timeout=60m --fatal sh -c 'exit 0'
# 		if [[ $CI_TAGS == *'apidiff'* ]]; then report_github_status "success" "API Diff" "Skipped." || true; fi
# 		exit 0
# 	fi

#     rm pr-files.txt
# fi

make_timeout=300m
gnumake=$(which gmake || which gnumake || which make)

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
if [[ ${CI_TAGS} == *'win-i386'* ]]; then PLATFORM=Win32; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=i686-w64-mingw32 --enable-btls"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/Win32/bin/Release/mono-sgen.exe"; fi
if [[ ${CI_TAGS} == *'win-amd64'* ]]; then PLATFORM=x64; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=x86_64-w64-mingw32 --disable-boehm --enable-btls"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/x64/bin/Release/mono-sgen.exe"; fi
if [[ ${CI_TAGS} == *'freebsd-amd64'* ]]; then export CC="clang"; export CXX="clang++"; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --disable-dtrace --disable-boehm ac_cv_header_sys_inotify_h=no ac_cv_func_inotify_init=no ac_cv_func_inotify_add_watch=no ac_cv_func_inotify_rm_watch=no"; fi
if [[ ${CI_TAGS} == *'make-install'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm --prefix=${MONO_REPO_ROOT}/tmp/monoprefix"; fi

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
elif [[ ${CI_TAGS} == *'winaot_llvm'* ]];        then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=winaot_llvm";
elif [[ ${CI_TAGS} == *'aot_llvm'* ]];           then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=aot_llvm ";
elif [[ ${CI_TAGS} == *'jit_llvm'* ]];           then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes"; export MONO_ENV_OPTIONS="$MONO_ENV_OPTIONS --llvm";
elif [[ ${CI_TAGS} == *'fullaotinterp_llvm'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=fullaotinterp_llvm";
elif [[ ${CI_TAGS} == *'fullaotinterp'* ]];      then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=fullaotinterp";
elif [[ ${CI_TAGS} == *'winaotinterp'* ]];       then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=winaotinterp";
elif [[ ${CI_TAGS} == *'winaotinterp_llvm'* ]];  then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-llvm=yes --with-runtime-preset=winaotinterp_llvm";
elif [[ ${CI_TAGS} == *'fullaot'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=fullaot";
elif [[ ${CI_TAGS} == *'hybridaot'* ]];          then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=hybridaot";
elif [[ ${CI_TAGS} == *'winaot'* ]];             then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=winaot";
elif [[ ${CI_TAGS} == *'aot'* ]];                then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=aot";
elif [[ ${CI_TAGS} == *'bitcodeinterp'* ]];      then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=bitcodeinterp"; export PATH="$PATH:${MONO_REPO_ROOT}/llvm/usr/bin";
elif [[ ${CI_TAGS} == *'bitcode'* ]];            then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=bitcode"; export PATH="$PATH:${MONO_REPO_ROOT}/llvm/usr/bin";
elif [[ ${CI_TAGS} == *'acceptance-tests'* ]];   then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --prefix=${MONO_REPO_ROOT}/tmp/mono-acceptance-tests --with-sgen-default-concurrent=yes";
elif [[ ${CI_TAGS} == *'all-profiles'* ]];       then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --with-runtime-preset=all";
elif [[ ${CI_TAGS} == *'compile-msbuild-source'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --prefix=/tmp/mono-from-source";
fi

if [ -x "/usr/bin/dpkg-architecture" ];
	then
	EXTRA_CONF_FLAGS="$EXTRA_CONF_FLAGS --host=`/usr/bin/dpkg-architecture -qDEB_HOST_GNU_TYPE`"
	#force build arch = dpkg arch, sometimes misdetected
	mkdir -p ~/.config/.mono/
	wget -O- https://download.mono-project.com/test/new-certs.tgz | tar zx -C ~/.config/.mono/
fi

if [[ ${CI_TAGS} == *'cxx'* ]]; then
	EXTRA_CONF_FLAGS="$EXTRA_CONF_FLAGS -enable-cxx"
	MSBUILD_CXX="/p:MONO_COMPILE_AS_CPP=true"
fi

if [[ ${CI_TAGS} == *'microbench'* ]]; then
	EXTRA_CONF_FLAGS="$EXTRA_CONF_FLAGS --with-profile4_x=yes"
fi

if [[ ${CI_TAGS} == *'win-'* ]];
then
	mkdir -p ~/.config/.mono/
	wget -qO- https://download.mono-project.com/test/new-certs.tgz| tar zx -C ~/.config/.mono/
fi

if [[ ${CI_TAGS} == *'ccache'* ]];
then
	# CCACHE_DIR should be set to a directory outside the chroot which holds the cache
	CCACHE=$(which ccache)
	if [ "$CCACHE" ]; then
		if [[ ${CI_TAGS} == *'osx-'* ]]; then
			export CC="ccache clang"
			export CXX="ccache clang++"
		else
			export CC="ccache gcc"
			export CXX="ccache g++"
		fi
	fi
fi

if [[ ${CI_TAGS} != *'mac-sdk'* ]]; # Mac SDK builds Mono itself
	then
	echo ./autogen.sh CFLAGS="$CFLAGS $EXTRA_CFLAGS" CXXFLAGS="$CXXFLAGS $EXTRA_CXXFLAGS" LDFLAGS="$LDFLAGS $EXTRA_LDFLAGS" $EXTRA_CONF_FLAGS
	${TESTCMD} --label=configure --timeout=60m --fatal ./autogen.sh CFLAGS="$CFLAGS $EXTRA_CFLAGS" CXXFLAGS="$CXXFLAGS $EXTRA_CXXFLAGS" LDFLAGS="$LDFLAGS $EXTRA_LDFLAGS" $EXTRA_CONF_FLAGS
fi

if [[ ${CI_TAGS} == *'msvc142'* ]]; then
    export VS_PLATFORMTOOLSETVERSION=v142
else
    export VS_PLATFORMTOOLSETVERSION=v140
fi
if [[ ${CI_TAGS} == *'win-i386'* ]];
    then
    # only build boehm on w32 (only windows platform supporting boehm).
    ${TESTCMD} --label=make-msvc --timeout=60m --fatal ./msvc/run-msbuild.sh "build" "${PLATFORM}" "release" "boehm" "/p:PlatformToolset=${VS_PLATFORMTOOLSETVERSION} ${MSBUILD_CXX}"
fi
if [[ ${CI_TAGS} == *'win-'* ]];
    then
    ${TESTCMD} --label=make-msvc-sgen --timeout=60m --fatal ./msvc/run-msbuild.sh "build" "${PLATFORM}" "release" "sgen" "/p:PlatformToolset=${VS_PLATFORMTOOLSETVERSION} ${MSBUILD_CXX}"
fi

if [[ ${CI_TAGS} == *'win-amd64'* ]];
    then
    # The AOT compiler on Windows requires Visual Studio's clang.exe and link.exe.
    # Depending on codegen (JIT/LLVM) it might also need platform specific libraries.
    # Use a wrapper script that will make sure to setup full VS MSVC environment if
    # needed when running mono-sgen.exe as AOT compiler.
    export MONO_EXECUTABLE_WRAPPER="${MONO_REPO_ROOT}/msvc/build/sgen/x64/bin/Release/mono-sgen-msvc.sh"
fi

if [[ ${CI_TAGS} == *'monolite'* ]]; then make get-monolite-latest; fi

make_parallelism="-j ${CI_CPU_COUNT}"
if [[ ${CI_TAGS} == *'linux-ppc64el'* ]]; then make_parallelism=-j1; fi

make_continue=
if [[ ${CI_TAGS} == *'checked-all'* ]]; then make_continue=-k; fi

if [[ ${CI_TAGS} != *'mac-sdk'* ]]; # Mac SDK builds Mono itself
    then
    build_error=0
    ${TESTCMD} --label=make --timeout=${make_timeout} --fatal make ${make_parallelism} ${make_continue} -w V=1 || build_error=1

    if [[ ${build_error} != 0 ]]; then
        echo "ERROR: The Mono build failed."
        exit ${build_error}
    fi
fi

if [[ ${CI_TAGS} == *'checked-coop'* ]]; then export MONO_CHECK_MODE=gc,thread; fi
if [[ ${CI_TAGS} == *'checked-all'* ]]; then export MONO_CHECK_MODE=all; fi

if [[ ${CI_TAGS} == *'hardened-runtime'* ]]; then codesign -s - -fv -o runtime --entitlements ${MONO_REPO_ROOT}/mono/mini/mac-entitlements.plist ${MONO_REPO_ROOT}/mono/mini/mono-sgen; fi

export MONO_ENV_OPTIONS="$MONO_ENV_OPTIONS $MONO_TEST_ENV_OPTIONS"

if   [[ ${CI_TAGS} == *'acceptance-tests'* ]];         then ${MONO_REPO_ROOT}/scripts/ci/run-test-acceptance-tests.sh;
elif [[ ${CI_TAGS} == *'microbench'* ]];               then ${MONO_REPO_ROOT}/scripts/ci/run-test-microbench.sh;
elif [[ ${CI_TAGS} == *'profiler-stress-tests'* ]];    then ${MONO_REPO_ROOT}/scripts/ci/run-test-profiler-stress-tests.sh;
elif [[ ${CI_TAGS} == *'stress-tests'* ]];             then ${MONO_REPO_ROOT}/scripts/ci/run-test-stress-tests.sh;
elif [[ ${CI_TAGS} == *'interpreter'* ]];              then ${MONO_REPO_ROOT}/scripts/ci/run-test-interpreter.sh;
elif [[ ${CI_TAGS} == *'mcs-compiler'* ]];             then ${MONO_REPO_ROOT}/scripts/ci/run-test-mcs.sh;
elif [[ ${CI_TAGS} == *'mac-sdk'* ]];                  then ${MONO_REPO_ROOT}/scripts/ci/run-test-mac-sdk.sh;
elif [[ ${CI_TAGS} == *'compile-msbuild-source'* ]];   then ${MONO_REPO_ROOT}/scripts/ci/run-test-msbuild.sh;
elif [[ ${CI_TAGS} == *'make-install'* ]];             then ${MONO_REPO_ROOT}/scripts/ci/run-test-make-install.sh;
elif [[ ${CI_TAGS} == *'compiler-server-tests'* ]];          then ${MONO_REPO_ROOT}/scripts/ci/run-test-compiler-server.sh;
elif [[ ${CI_TAGS} == *'no-tests'* ]];                 then echo "Skipping tests.";
else make check-ci;
fi

if [[ $CI_TAGS == *'apidiff'* ]]; then ${TESTCMD} --label=apidiff --timeout=15m --fatal make -w -C mcs -j ${CI_CPU_COUNT} mono-api-diff; fi
