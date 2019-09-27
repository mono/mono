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

if [[ ${CI_TAGS} == *'pull-request'* ]]; then
	# Skip lanes which are not affected by the PR
	wget -O pr-contents.diff "${ghprbPullLink}.diff"
	grep '^diff' pr-contents.diff > pr-files.txt
	echo "Files affected by the PR:"
	cat pr-files.txt

	# FIXME: Add more
	skip=false
	skip_step=""
	if ! grep -q -v a/netcore pr-files.txt; then
		skip_step="NETCORE"
		skip=true
	fi
	if ! grep -q -v a/mono/mini/mini-ppc pr-files.txt; then
		skip_step="PPC"
		skip=true
	fi
	if ! grep -q -v a/scripts/ci/provisioning pr-files.txt; then
		skip_step="CI provisioning scripts"
		skip=true
	fi
	if ! grep -q -v a/sdks/wasm pr-files.txt; then
		if [[ ${CI_TAGS} == *'webassembly'* ]] || [[ ${CI_TAGS} == *'wasm'* ]]; then
			true
		else
			skip_step="WASM"
			skip=true
		fi
	fi
	if [ $skip = true ]; then
		${TESTCMD} --label="Skipped on ${skip_step}." --timeout=60m --fatal sh -c 'exit 0'
		if [[ $CI_TAGS == *'apidiff'* ]]; then report_github_status "success" "API Diff" "Skipped." || true; fi
		if [[ $CI_TAGS == *'csprojdiff'* ]]; then report_github_status "success" "Project Files Diff" "Skipped." || true; fi
		exit 0
	fi

    rm pr-files.txt
fi

helix_set_env_vars
helix_send_build_start_event "build/source/$MONO_HELIX_TYPE/"

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
if [[ ${CI_TAGS} == *'win-amd64'* && ${CI_TAGS} != *'sdks-android'* ]]; then PLATFORM=x64; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --host=x86_64-w64-mingw32 --disable-boehm --enable-btls"; export MONO_EXECUTABLE="${MONO_REPO_ROOT}/msvc/build/sgen/x64/bin/Release/mono-sgen.exe"; fi
if [[ ${CI_TAGS} == *'freebsd-amd64'* ]]; then export CC="clang"; export CXX="clang++"; EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --disable-dtrace --disable-boehm ac_cv_header_sys_inotify_h=no ac_cv_func_inotify_init=no ac_cv_func_inotify_add_watch=no ac_cv_func_inotify_rm_watch=no"; fi
if [[ ${CI_TAGS} == *'make-install'* ]]; then EXTRA_CONF_FLAGS="${EXTRA_CONF_FLAGS} --enable-loadedllvm --prefix=${MONO_REPO_ROOT}/tmp/monoprefix"; fi

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

if [[ ${CI_TAGS} == *'sdks-llvm'* ]]; then
	${TESTCMD} --label=archive --timeout=120m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-llvm-llvm{,win}64 NINJA=
	exit 0
fi

if [[ ${CI_TAGS} == *'sdks-ios'* ]];
   then
        # configuration on our bots
        if [[ ${CI_TAGS} == *'xcode11'* ]]; then
            export XCODE_DIR=/Applications/Xcode11.app/Contents/Developer
            export MACOS_VERSION=10.15
            export IOS_VERSION=13.0
            export TVOS_VERSION=13.0
            export WATCHOS_VERSION=6.0
            export WATCHOS64_32_VERSION=6.0
        else
            export XCODE_DIR=/Applications/Xcode101.app/Contents/Developer
            export MACOS_VERSION=10.14
            export IOS_VERSION=12.1
            export TVOS_VERSION=12.1
            export WATCHOS_VERSION=5.1
            export WATCHOS64_32_VERSION=5.1
        fi

        # retrieve selected Xcode version
        /usr/libexec/PlistBuddy -c 'Print :ProductBuildVersion' ${XCODE_DIR}/../version.plist > xcode_version.txt

        # make sure we embed the correct path into the PDBs
        export MONOTOUCH_MCS_FLAGS=-pathmap:${MONO_REPO_ROOT}/=/Library/Frameworks/Xamarin.iOS.framework/Versions/Current/src/Xamarin.iOS/

        echo "ENABLE_IOS=1" > sdks/Make.config
        echo "ENABLE_MAC=1" >> sdks/Make.config
        if [[ ${CI_TAGS} == *'cxx'* ]]; then
            echo "ENABLE_CXX=1" >> sdks/Make.config
        fi
        if [[ ${CI_TAGS} == *'debug'* ]]; then
            echo "CONFIGURATION=debug" >> sdks/Make.config
        fi

	   export device_test_suites="Mono.Runtime.Tests System.Core"

	   ${TESTCMD} --label=configure --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds configure-ios NINJA=
	   ${TESTCMD} --label=build     --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds build-ios     NINJA=
	   ${TESTCMD} --label=archive   --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-ios   NINJA=

        if [[ ${CI_TAGS} != *'no-tests'* ]]; then
            ${TESTCMD} --label=run-sim --timeout=20m $gnumake -C sdks/ios run-ios-sim-all
            ${TESTCMD} --label=build-ios-dev --timeout=60m $gnumake -C sdks/ios build-ios-dev-all
            if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
                for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-${suite} --timeout=10m $gnumake -C sdks/ios run-ios-dev-${suite}; done
            fi
            ${TESTCMD} --label=build-ios-dev-llvm --timeout=60m $gnumake -C sdks/ios build-ios-dev-llvm-all
            if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
                for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-llvm-${suite} --timeout=10m $gnumake -C sdks/ios run-ios-dev-${suite}; done
            fi
            ${TESTCMD} --label=build-ios-dev-interp-only --timeout=60m $gnumake -C sdks/ios build-ios-dev-interp-only-all
            if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
                for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-interp-only-${suite} --timeout=10m $gnumake -C sdks/ios run-ios-dev-${suite}; done
            fi
            ${TESTCMD} --label=build-ios-dev-interp-mixed --timeout=60m $gnumake -C sdks/ios build-ios-dev-interp-mixed-all
            if [[ ${CI_TAGS} == *'run-device-tests'* ]]; then
                for suite in ${device_test_suites}; do ${TESTCMD} --label=run-ios-dev-interp-mixed-${suite} --timeout=10m $gnumake -C sdks/ios run-ios-dev-${suite}; done
            fi
        fi
	   exit 0
fi

if [[ ${CI_TAGS} == *'sdks-mac'* ]];
then
    # configuration on our bots
    if [[ ${CI_TAGS} == *'xcode11'* ]]; then
        export XCODE_DIR=/Applications/Xcode11.app/Contents/Developer
        export MACOS_VERSION=10.15
    else
        export XCODE_DIR=/Applications/Xcode101.app/Contents/Developer
        export MACOS_VERSION=10.14
    fi

    # retrieve selected Xcode version
    /usr/libexec/PlistBuddy -c 'Print :ProductBuildVersion' ${XCODE_DIR}/../version.plist > xcode_version.txt

    # make sure we embed the correct path into the PDBs
    export XAMMAC_MCS_FLAGS=-pathmap:${MONO_REPO_ROOT}/=/Library/Frameworks/Xamarin.Mac.framework/Versions/Current/src/Xamarin.Mac/

    echo "ENABLE_MAC=1" > sdks/Make.config
    if [[ ${CI_TAGS} == *'cxx'* ]]; then
        echo "ENABLE_CXX=1" >> sdks/Make.config
    fi
    if [[ ${CI_TAGS} == *'debug'* ]]; then
        echo "CONFIGURATION=debug" >> sdks/Make.config
    fi

    ${TESTCMD} --label=configure --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds configure-mac NINJA=
    ${TESTCMD} --label=build     --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds build-mac     NINJA=
    ${TESTCMD} --label=archive   --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-mac   NINJA=

    exit 0
fi

if [[ ${CI_TAGS} == *'sdks-android'* ]];
   then
        echo "ENABLE_ANDROID=1" > sdks/Make.config
        echo "DISABLE_CCACHE=1" >> sdks/Make.config
        if [[ ${CI_TAGS} == *'cxx'* ]]; then
            echo "ENABLE_CXX=1" >> sdks/Make.config
        fi
        if [[ ${CI_TAGS} == *'debug'* ]]; then
            echo "CONFIGURATION=debug" >> sdks/Make.config
        fi

        # TODO: provision-android on Windows.
        if [[ ${CI_TAGS} != *'win-'* ]]; then
        # For some very strange reasons, `make -C sdks/android accept-android-license` get stuck when invoked through ${TESTCMD}
            # but doesn't get stuck when called via the shell, so let's just call it here now.
            ${TESTCMD} --label=provision-android --timeout=120m --fatal $gnumake -j ${CI_CPU_COUNT} -C sdks/builds provision-android && $gnumake -C sdks/android accept-android-license
        fi
        ${TESTCMD} --label=provision-mxe --timeout=240m --fatal $gnumake -j ${CI_CPU_COUNT} -C sdks/builds provision-mxe

        ${TESTCMD} --label=configure --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds configure-android NINJA= IGNORE_PROVISION_ANDROID=1 IGNORE_PROVISION_MXE=1
        ${TESTCMD} --label=build     --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds build-android     NINJA= IGNORE_PROVISION_ANDROID=1 IGNORE_PROVISION_MXE=1
        ${TESTCMD} --label=archive   --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-android   NINJA= IGNORE_PROVISION_ANDROID=1 IGNORE_PROVISION_MXE=1

        if [[ ${CI_TAGS} != *'no-tests'* ]]; then
            ${TESTCMD} --label=mini --timeout=20m $gnumake -C sdks/android check-Mono.Runtime.Tests
            ${TESTCMD} --label=corlib --timeout=20m $gnumake -C sdks/android check-corlib
            ${TESTCMD} --label=System --timeout=20m $gnumake -C sdks/android check-System
            ${TESTCMD} --label=System.Core --timeout=20m $gnumake -C sdks/android check-System.Core
            ${TESTCMD} --label=System.Data --timeout=20m $gnumake -C sdks/android check-System.Data
            ${TESTCMD} --label=System.IO.Compression.FileSystem --timeout=20m $gnumake -C sdks/android check-System.IO.Compression.FileSystem
            ${TESTCMD} --label=System.IO.Compression --timeout=20m $gnumake -C sdks/android check-System.IO.Compression
            ${TESTCMD} --label=System.Json --timeout=20m $gnumake -C sdks/android check-System.Json
            ${TESTCMD} --label=System.Net.Http --timeout=20m $gnumake -C sdks/android check-System.Net.Http
            ${TESTCMD} --label=System.Numerics --timeout=20m $gnumake -C sdks/android check-System.Numerics
            ${TESTCMD} --label=System.Runtime.Serialization --timeout=20m $gnumake -C sdks/android check-System.Runtime.Serialization
            ${TESTCMD} --label=System.ServiceModel.Web --timeout=20m $gnumake -C sdks/android check-System.ServiceModel.Web
            ${TESTCMD} --label=System.Transactions --timeout=20m $gnumake -C sdks/android check-System.Transactions
            ${TESTCMD} --label=System.Xml --timeout=20m $gnumake -C sdks/android check-System.Xml
            ${TESTCMD} --label=System.Xml.Linq --timeout=20m $gnumake -C sdks/android check-System.Xml.Linq
            ${TESTCMD} --label=Mono.CSharp --timeout=20m $gnumake -C sdks/android check-Mono.CSharp
            # ${TESTCMD} --label=Mono.Data.Sqlite --timeout=20m $gnumake -C sdks/android check-Mono.Data.Sqlite
            ${TESTCMD} --label=Mono.Data.Tds --timeout=20m $gnumake -C sdks/android check-Mono.Data.Tds
            ${TESTCMD} --label=Mono.Security --timeout=20m $gnumake -C sdks/android check-Mono.Security
            ${TESTCMD} --label=Mono.Debugger.Soft --timeout=20m $gnumake -C sdks/android check-Mono.Debugger.Soft
        fi
        exit 0
fi

if [[ ${CI_TAGS} == *'webassembly'* ]] || [[ ${CI_TAGS} == *'wasm'* ]];
   then
        echo "ENABLE_WASM=1" > sdks/Make.config
        echo "ENABLE_WINDOWS=1" >> sdks/Make.config
        if [[ ${CI_TAGS} == *'cxx'* ]]; then
            echo "ENABLE_CXX=1" >> sdks/Make.config
        fi
        if [[ ${CI_TAGS} == *'debug'* ]]; then
            echo "CONFIGURATION=debug" >> sdks/Make.config
        fi
        echo "ENABLE_WASM_DYNAMIC_RUNTIME=1" >> sdks/Make.config
        #echo "ENABLE_WASM_THREADS=1" >> sdks/Make.config

	   export aot_test_suites="System.Core"
	   export mixed_test_suites="System.Core"
	   export xunit_test_suites="System.Core corlib System Microsoft.CSharp System.Data System.IO.Compression System.Net.Http.UnitTests System.Numerics System.Runtime.Serialization System.Security System.Xml System.Xml.Linq"

	   ${TESTCMD} --label=provision --timeout=20m --fatal $gnumake --output-sync=recurse --trace -C sdks/builds provision-wasm

	   ${TESTCMD} --label=configure --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds configure-wasm NINJA=
	   ${TESTCMD} --label=build     --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds build-wasm     NINJA=
	   ${TESTCMD} --label=archive   --timeout=180m --fatal $gnumake -j ${CI_CPU_COUNT} --output-sync=recurse --trace -C sdks/builds archive-wasm   NINJA=

        if [[ ${CI_TAGS} != *'no-tests'* ]]; then
            ${TESTCMD} --label=wasm-build --timeout=20m --fatal $gnumake -j ${CI_CPU_COUNT} -C sdks/wasm build
            ${TESTCMD} --label=mini --timeout=20m $gnumake -C sdks/wasm run-all-mini
            ${TESTCMD} --label=v8-corlib --timeout=20m $gnu$gnumake -C sdks/wasm run-v8-corlib
            #The following tests are not passing yet, so enabling them would make us perma-red
            #${TESTCMD} --label=mini-system --timeout=20m $gnu$gnumake -C sdks/wasm run-all-system
            ${TESTCMD} --label=system-core --timeout=20m $gnumake -C sdks/wasm run-all-System.Core
            for suite in ${xunit_test_suites}; do ${TESTCMD} --label=xunit-${suite} --timeout=30m $gnumake -C sdks/wasm run-${suite}-xunit; done
            # disable for now until https://github.com/mono/mono/pull/13622 goes in
            #${TESTCMD} --label=debugger --timeout=20m $gnumake -C sdks/wasm test-debugger
            ${TESTCMD} --label=browser --timeout=20m $gnumake -C sdks/wasm run-browser-tests
            #${TESTCMD} --label=browser-threads --timeout=20m $gnumake -C sdks/wasm run-browser-threads-tests
            ${TESTCMD} --label=aot-mini --timeout=20m $gnumake -j ${CI_CPU_COUNT} -C sdks/wasm run-aot-mini
            ${TESTCMD} --label=build-aot-all --timeout=20m $gnumake -j ${CI_CPU_COUNT} -C sdks/wasm build-aot-all
            for suite in ${aot_test_suites}; do ${TESTCMD} --label=run-aot-${suite} --timeout=10m $gnumake -C sdks/wasm run-aot-${suite}; done
            for suite in ${mixed_test_suites}; do ${TESTCMD} --label=run-aot-mixed-${suite} --timeout=10m $gnumake -C sdks/wasm run-aot-mixed-${suite}; done
            #${TESTCMD} --label=check-aot --timeout=20m $gnumake -C sdks/wasm check-aot
            ${TESTCMD} --label=package --timeout=20m $gnumake -C sdks/wasm package
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
    ${TESTCMD} --label=make-msvc --timeout=60m --fatal ./msvc/run-msbuild.sh "build" "${PLATFORM}" "release" "/p:PlatformToolset=v140 /p:MONO_TARGET_GC=boehm ${MSBUILD_CXX}"
fi
if [[ ${CI_TAGS} == *'win-'* ]];
    then
    ${TESTCMD} --label=make-msvc-sgen --timeout=60m --fatal ./msvc/run-msbuild.sh "build" "${PLATFORM}" "release" "/p:PlatformToolset=v140 /p:MONO_TARGET_GC=sgen ${MSBUILD_CXX}"
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
    helix_send_build_done_event "build/source/$MONO_HELIX_TYPE/" $build_error

    if [[ ${build_error} != 0 ]]; then
        echo "ERROR: The Mono build failed."
        ${MONO_REPO_ROOT}/scripts/ci/run-upload-sentry.sh
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
elif [[ ${CI_TAGS} == *'helix-tests'* ]];              then ${MONO_REPO_ROOT}/scripts/ci/run-test-helix.sh;
elif [[ ${CI_TAGS} == *'compile-msbuild-source'* ]];   then ${MONO_REPO_ROOT}/scripts/ci/run-test-msbuild.sh;
elif [[ ${CI_TAGS} == *'make-install'* ]];             then ${MONO_REPO_ROOT}/scripts/ci/run-test-make-install.sh;
elif [[ ${CI_TAGS} == *'compiler-server-tests'* ]];          then ${MONO_REPO_ROOT}/scripts/ci/run-test-compiler-server.sh;
elif [[ ${CI_TAGS} == *'no-tests'* ]];                 then echo "Skipping tests.";
else make check-ci;
fi

if [[ $CI_TAGS == *'apidiff'* ]]; then
    if ${TESTCMD} --label=apidiff --timeout=15m --fatal make -w -C mcs -j ${CI_CPU_COUNT} mono-api-diff
    then report_github_status "success" "API Diff" "No public API changes found." || true
    else report_github_status "error" "API Diff" "The public API changed." "$BUILD_URL/Public_20API_20Diff/" || true
    fi
fi
if [[ $CI_TAGS == *'csprojdiff'* ]]; then
    make update-solution-files
    if ${TESTCMD} --label=csprojdiff --timeout=5m --fatal make -w -C mcs mono-csproj-diff
    then report_github_status "success" "Project Files Diff" "No csproj file changes found." || true
    else report_github_status "error" "Project Files Diff" "The csproj files changed." "$BUILD_URL/Project_20Files_20Diff/" || true
    fi
fi
