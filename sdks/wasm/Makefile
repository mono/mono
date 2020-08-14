TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

all: build

# Required by emscripten:
# https://github.com/emscripten-core/emscripten/issues/10551
# PYTHONUTF8 requires python 3.7, so set LC_ALL as well
export PYTHONUTF8=1
export LC_ALL=C.UTF-8

MAKEFLAGS += --no-builtin-rules
#emcc has lots of bash'isms
SHELL:=/bin/bash

#Use either 'release' or 'debug' depending on what you need
DRIVER_CONF=release

# This is duplicated from configure.ac but we can't include config.make as
# it might not be generated yet
CSC_LOCATION=$(TOP)/external/roslyn-binaries/Microsoft.Net.Compilers/3.5.0/csc.exe

EMSCRIPTEN_SDK_DIR?=$(TOP)/sdks/builds/toolchains/emsdk
EMCC=source $(CURDIR)/emsdk_env.sh && emcc
EMCC_DYNAMIC=EMCC_FORCE_STDLIBS=1 $(EMCC)
WASM_BCL_DIR=$(TOP)/sdks/out/wasm-bcl/wasm
WASM_RUNTIME_DIR=$(TOP)/sdks/out/wasm-runtime-release
MINI_PATH=$(TOP)/mono/mini
CSC?= MONO_PATH=$(TOP)/mcs/class/lib/build $(TOP)/sdks/builds/bcl/runtime/mono-wrapper $(CSC_LOCATION)
CSC_FLAGS := /debug:portable /noconfig /nostdlib /nologo /langversion:latest
API_REFS=$(TOP)/external/binary-reference-assemblies/v4.6

PACKAGER=mono --debug packager.exe --no-native-strip

MSBUILD_QUIET_ARGS=--nologo -v q -consoleloggerparameters:NoSummary

ifeq ($(HOST_PLATFORM),win32)
DOTNET_CMD := $(shell powershell -ExecutionPolicy Bypass -Command "./init-tools.ps1")/dotnet.exe
DOTNET_CMD := "$(subst \,/,$(DOTNET))"
else
DOTNET_CMD := $(shell ./init-tools.sh | tail -1)
endif

DOTNET_PACK=$(DOTNET_CMD) pack
DOTNET_BUILD=$(DOTNET_CMD) build

MINI_TEST_FILES= \
    TestDriver.cs   \
    aot-tests.cs \
    arrays.cs       \
    basic-calls.cs  \
    basic-float.cs  \
    basic-long.cs   \
    basic-math.cs   \
    basic.cs        \
    exceptions.cs   \
    generics.cs     \
    gshared.cs      \
    objects.cs	\
	builtin-types.cs	\
	devirtualization.cs	\
	mixed.cs	\
	gc-test.cs

APP_SOURCES = \
	main.cs	\
	incrementalrunner.cs

BINDING_TEST_SOURCES = \
	bindings-test.cs

MINI_TEST_SOURCES = $(patsubst %,$(MINI_PATH)/%,$(MINI_TEST_FILES))

WASM_FRAMEWORK=$(TOP)/sdks/wasm/framework
WASM_FRAMEWORK_SOURCE=$(TOP)/sdks/wasm/framework/src
WASM_FRAMEWORK_SOURCES=$(WASM_FRAMEWORK_SOURCE)/**/*
WASM_FRAMEWORK_DEPS=/r:$(WASM_FRAMEWORK)/WebAssembly.Bindings.dll /r:$(WASM_FRAMEWORK)/System.Net.Http.WebAssemblyHttpHandler.dll /r:$(WASM_FRAMEWORK)/WebAssembly.Net.WebSockets.dll

BROWSER_TEST=$(TOP)/sdks/wasm/tests/browser
BROWSER_TEST_THREADS=$(TOP)/sdks/wasm/tests/browser
BROWSER_TEST_DYNAMIC=$(TOP)/sdks/wasm/tests/browser
BROWSER_TEST_SOURCES=$(BROWSER_TEST)/*
BROWSER_TEST_ASSETS = \
    --asset=$(BROWSER_TEST)/http-spec.html   \
    --asset=$(BROWSER_TEST)/http-spec.js   \
    --asset=$(BROWSER_TEST)/core-bindings-spec.html   \
    --asset=$(BROWSER_TEST)/core-bindings-spec.js   \
    --asset=$(BROWSER_TEST)/issues-spec.html   \
    --asset=$(BROWSER_TEST)/issues-spec.js   \
    --asset=$(BROWSER_TEST)/zip-spec.html   \
    --asset=$(BROWSER_TEST)/zip-spec.js   \
    --asset=$(BROWSER_TEST)/NowIsTheTime.txt   \
    --asset=$(BROWSER_TEST)/resources/archive.zip   \
    --asset=$(BROWSER_TEST)/resources/test.nupkg   \

WASM_TESTS_SRC=$(TOP)/sdks/wasm/tests/src
WASM_TESTS_BIN=$(TOP)/sdks/wasm/bin

TEST_SOURCE_DIR=$(TOP)/sdks/wasm/tests/src/

UNAME := $(shell uname -s)
ifeq ($(UNAME),Linux)
	JSVU_OS=linux64
endif
ifeq ($(UNAME),Darwin)
	JSVU_OS=mac64
endif
.PHONY: jsup

NPM_VERSION=@^6.13.4
NPM=$(CURDIR)/node_modules/node/bin/node $(CURDIR)/node_modules/npm/bin/npm-cli.js

.stamp-npm-$(NPM_VERSION): 
	npm install npm$(NPM_VERSION)
	node_modules/npm/bin/npm-cli.js install
	touch $@

jsup: .stamp-jsvu
	node_modules/node/bin/node node_modules/jsvu/cli.js --os=$(JSVU_OS) --engines=javascriptcore,spidermonkey,v8

.stamp-jsvu: .stamp-npm-$(NPM_VERSION)
	node_modules/node/bin/node node_modules/jsvu/cli.js --os=$(JSVU_OS) --engines=javascriptcore,spidermonkey,v8
	touch $@

.PHONY: toolchain
toolchain: .stamp-jsvu

provision-emsdk:
	$(MAKE) -C ../builds provision-wasm

# emsdk_env.sh calls emsdk construct_env which is a bit slow so make a copy
emsdk_env.sh: | provision-emsdk
	cd $(EMSCRIPTEN_SDK_DIR) && ./emsdk construct_env > $(CURDIR)/emsdk_env.sh

# These builds overwrite their installed files
# forcing everything to be rebuilt so use a stamp file
.stamp-runtime: | provision-emsdk
	$(MAKE) -C ../builds package-wasm-runtime
	touch $@

.stamp-bcl:
	$(MAKE) -C ../builds package-wasm-bcl
	touch $@

# Use these targets to rebuild components
runtime:
	$(MAKE) -C ../builds package-wasm-runtime
	touch .stamp-runtime

cross:
	$(MAKE) -C ../builds package-wasm-cross

bcl:
	$(MAKE) -C ../builds package-wasm-bcl
	touch .stamp-bcl

tuner:
	$(MAKE) -C ../../mcs/tools/wasm-tuner PROFILE=wasm_tools
	cp ../../mcs/class/lib/wasm_tools/wasm-tuner.* ../out/wasm-bcl/wasm_tools/

MONO_LIBS = $(TOP)/sdks/out/wasm-runtime-release/lib/{libmono-ee-interp.a,libmono-native.a,libmono-icall-table.a,libmonosgen-2.0.a,libmono-ilgen.a}
MONO_THREADS_LIBS = $(TOP)/sdks/out/wasm-runtime-threads-release/lib/{libmono-ee-interp.a,libmono-native.a,libmono-icall-table.a,libmonosgen-2.0.a,libmono-ilgen.a}
MONO_DYNAMIC_LIBS = $(TOP)/sdks/out/wasm-runtime-dynamic-release/lib/{libmono-ee-interp.a,libmono-native.a,libmono-icall-table.a,libmonosgen-2.0.a,libmono-ilgen.a}

EMCC_FLAGS=--profiling-funcs -s ALLOW_MEMORY_GROWTH=1 -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString', 'addFunction']" -s USE_ZLIB=1 -s "EXPORTED_FUNCTIONS=['_putchar']" --source-map-base http://example.com  -emit-llvm -s FORCE_FILESYSTEM=1
EMCC_DEBUG_FLAGS =-g -Os -s ASSERTIONS=1
EMCC_RELEASE_FLAGS=-Oz --llvm-opts 2
EMCC_DYNAMIC_FLAGS=-s MAIN_MODULE=1 -s EXPORT_ALL=1 -s DISABLE_EXCEPTION_CATCHING=0 -s ALLOW_TABLE_GROWTH=1 -s USE_ZLIB=0 -emit-llvm -DWASM_SUPPORTS_DLOPEN
EMCC_THREADS_FLAGS=-s ALLOW_MEMORY_GROWTH=0 -s USE_PTHREADS=1 -s TOTAL_MEMORY=536870912 -pthread -Wl,--shared-memory,--no-check-features -s PTHREAD_POOL_SIZE=2

#
# Interpreter builds
#

MONO_LIBS = $(TOP)/sdks/out/wasm-runtime-release/lib/{libmono-ee-interp.a,libmono-native.a,libmonosgen-2.0.a,libmono-ilgen.a,libmono-icall-table.a}
MONO_DEPS = .stamp-runtime

# $(1) - name
# $(2) - runtime dir
# $(3) - EMCC_FLAGS
# $(4) - libs
# $(5) - extra deps
define InterpBuildTemplate

builds/$(1)/:
	mkdir -p $$@

builds/$(1)/dotnet.js: builds/$(1)/driver.o builds/$(1)/pinvoke.o builds/$(1)/corebindings.o builds/$(1)/zlib-helper.o src/library_mono.js src/binding_support.js src/dotnet_support.js $(5) | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(EMCC_FLAGS) $(3) --js-library src/library_mono.js --js-library src/binding_support.js --js-library src/dotnet_support.js builds/$(1)/driver.o builds/$(1)/corebindings.o builds/$(1)/zlib-helper.o builds/$(1)/pinvoke.o $(4) -o builds/$(1)/dotnet.js

builds/$(1)/driver.o: src/driver.c src/corebindings.c $(5) | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(EMCC_FLAGS) $(3) -Oz -DCORE_BINDINGS -I$(WASM_RUNTIME_DIR)/include/mono-2.0 src/driver.c -c -o $$@

builds/$(1)/pinvoke.o: src/pinvoke.c src/pinvoke-tables-default.h $(5) | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(EMCC_FLAGS) $(3) -Oz -DCORE_BINDINGS -I$(WASM_RUNTIME_DIR)/include/mono-2.0 src/pinvoke.c -c -o $$@

builds/$(1)/corebindings.o: src/corebindings.c | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(3) -Oz -I$(WASM_RUNTIME_DIR)/include/mono-2.0 src/corebindings.c -c -o $$@

builds/$(1)/zlib-helper.o: src/zlib-helper.c | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(3) -Oz -I$(WASM_RUNTIME_DIR)/include/mono-2.0 -s USE_ZLIB=1 src/zlib-helper.c -c -o $$@

build-native: builds/$(1)/dotnet.js

build-interp-$(1): builds/$(1)/dotnet.js

endef

ifndef DISABLE_WASM_NO_NETCORE
$(eval $(call InterpBuildTemplate,debug,wasm-runtime-release,$(EMCC_DEBUG_FLAGS),$(MONO_LIBS),$(MONO_DEPS)))
$(eval $(call InterpBuildTemplate,release,wasm-runtime-release,$(EMCC_RELEASE_FLAGS),$(MONO_LIBS),$(MONO_DEPS)))
ifdef ENABLE_WASM_THREADS
$(eval $(call InterpBuildTemplate,threads-debug,wasm-runtime-threads-release,$(EMCC_DEBUG_FLAGS) $(EMCC_THREADS_FLAGS),$(MONO_THREADS_LIBS),$(MONO_DEPS)))
$(eval $(call InterpBuildTemplate,threads-release,wasm-runtime-threads-release,$(EMCC_RELEASE_FLAGS) $(EMCC_THREADS_FLAGS),$(MONO_THREADS_LIBS),$(MONO_DEP)))
endif
ifdef ENABLE_WASM_DYNAMIC_RUNTIME
$(eval $(call InterpBuildTemplate,dynamic-debug,wasm-runtime-dynamic-release,$(EMCC_DEBUG_FLAGS) $(EMCC_DYNAMIC_FLAGS),$(MONO_DYNAMIC_LIBS),$(MONO_DEPS)))
$(eval $(call InterpBuildTemplate,dynamic-release,wasm-runtime-dynamic-release,$(EMCC_RELEASE_FLAGS) $(EMCC_DYNAMIC_FLAGS),$(MONO_DYNAMIC_LIBS),$(MONO_DEPS)))
endif
endif

#
# NETCORE TARGETS
#

#
# These require a separate dotnet/runtime repo
#

ifdef ENABLE_WASM_NETCORE

ifeq ($(ENABLE_WASM_NETCORE_LOCAL_BUILD),)
$(error "Only local builds using a separate dotnet/runtime repo are supported.")
endif

ifeq ($(DOTNET_REPO_PATH),)
$(error "Set the DOTNET_REPO_PATH variable for the netcore build to a checkout of the dotnet/runtime repo.")
endif

DOTNET=$(DOTNET_REPO_PATH)/dotnet.sh
RUNTIMEPACK_DIR=$(DOTNET_REPO_PATH)/artifacts/bin/lib-runtime-packs/net5.0-Browser-Release-wasm

.PHONY: build-netcore
build-netcore: | provision-emsdk
	EMSDK_PATH=$(EMSCRIPTEN_SDK_DIR) && make -C $(DOTNET_REPO_PATH)/src/mono/wasm build

samples/hello/bin/Debug/net5.0/hello.dll: samples/hello/Program.cs
	$(DOTNET) build samples/hello -f net5.0 $(MSBUILD_QUIET_ARGS)

build-interp-netcore-hello: packager.exe samples/hello/bin/Debug/net5.0/hello.dll
	$(PACKAGER) --debugrt --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/interp-netcore-hello --nobinding --builddir=obj/interp-netcore-hello --framework=net5.0 --runtimepack-dir=$(RUNTIMEPACK_DIR) --template=runtime-tests.js --linker samples/hello/bin/Debug/net5.0/hello.dll
	ninja -v -C obj/interp-netcore-hello

bin/interp-netcore-hello/dotnet.js: build-interp-netcore-hello

run-interp-netcore-hello: bin/interp-netcore-hello/dotnet.js
	(cd bin/interp-netcore-hello && $(SM) runtime.js --run hello.dll)

build-aot-hello-netcore: packager.exe samples/hello/bin/Debug/net5.0/hello.dll
	$(PACKAGER) --native-strip --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-hello-netcore --nobinding --builddir=obj/aot-hello-netcore --framework=net5.0 --runtimepack-dir=$(RUNTIMEPACK_DIR) --template=runtime-tests.js --aot --linker --pinvoke-libs=libSystem.Native samples/hello/bin/Debug/net5.0/hello.dll
	ninja -v -C obj/aot-hello-netcore

bin/aot-hello-netcore/dotnet.js: build-aot-hello-netcore

run-aot-hello-netcore: bin/aot-hello-netcore/dotnet.js
	(cd bin/aot-hello-netcore && $(RUN_V8) --experimental-wasm-simd runtime.js -- --run hello.dll)

#
# XUNIT test support
#
# This depends on the tests being built using build.sh --subset Libs.Tests -c release

##
# Parameters:
# $(1) - make target suffix
# $(2) - directory name
# $(3) - test directory
# $(4) - test assembly
# $(5) - command line arguments
define NetcoreTestTemplate

ifeq ($(3),Common.Tests)
$(1)_PATH=$(DOTNET_REPO_PATH)/artifacts/bin/$(3)/netcoreapp5.0-OSX-Release
else
$(1)_PATH=$(DOTNET_REPO_PATH)/artifacts/bin/$(3)/netcoreapp5.0-Release
endif

bin/$(2)/dotnet.js: packager.exe runtime-tests.js corefx-exclusions.rsp
	cd xunit-runner && ../../../.dotnet/dotnet build $(MSBUILD_QUIET_ARGS)
	$(PACKAGER) --debugrt --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --search-path=$(COREFX_TESTS_DIR)/$(3) --template=runtime-tests.js --appdir=bin/$(2) --builddir=obj/$(2) --asset=corefx-exclusions.rsp --framework=netcoreapp5.0 --netcore-sdkdir=$(NETCOREAPP_DIR) --search-path=$$($(1)_PATH) --search-path=$$($(1)_PATH) xunit-runner/bin/Debug/netcoreapp5.0/xunit-runner.dll $(5) $$($(1)_PATH)/$(4)
	ninja -v -C obj/$(2)

build-$(1): bin/$(2)/dotnet.js

run-$(1): bin/$(2)/dotnet.js
	(cd bin/$(2) && $$(SM) runtime.js --enable-gc --run xunit-runner.dll $(4) `cat corefx-exclusions.rsp | grep -v '#'`)

endef

include corefx-test-suites.mk

# Passing testsuites after exclusions
NETCORE_COREFX_PASSING_TEST_SUITES = \
	Common.Tests \
	Invariant.Tests \
	Microsoft.CSharp.Tests \
	Microsoft.VisualBasic.Core.Tests \
	Microsoft.Win32.Primitives.Tests \
	System.AppContext.Tests \
	System.Buffers.Tests \
	System.Collections.Concurrent.Tests \
	System.Collections.Immutable.Tests \
	System.Collections.NonGeneric.Tests \
	System.Collections.Specialized.Tests \
	System.Collections.Tests \
	System.IO.UnmanagedMemoryStream.Tests \
	System.Json.Tests \
	System.Memory.Tests \
	System.Numerics.Vectors.Tests \
	System.Runtime.CompilerServices.Unsafe.Tests \
	System.Runtime.CompilerServices.VisualC.Tests \
	System.Runtime.Extensions.Tests \
	System.Runtime.Handles.Tests \
	System.Runtime.Tests

System.IO.Compression.ZipFile.Tests_PACKAGER_ARGS=--pinvoke-libs=System.IO.Compression.Native --native-lib=$(TOP)/sdks/out/wasm-runtime-netcore-release/lib/System.IO.Compression.Native.bc --embed-file $(COREFX_TESTS_DIR)/System.IO.Compression.ZipFile.Tests/ZipTestData@ZipTestData
System.IO.Compression.Brotli.Tests_PACKAGER_ARGS=--pinvoke-libs=System.IO.Compression.Native --native-lib=$(TOP)/sdks/out/wasm-runtime-netcore-release/lib/System.IO.Compression.Native.bc --embed-file $(COREFX_TESTS_DIR)/System.IO.Compression.Brotli.Tests/UncompressedTestFiles@UncompressedTestFiles --embed-file $(COREFX_TESTS_DIR)/System.IO.Compression.Brotli.Tests/BrotliTestData@BrotliTestData

# Test targets are named run-netcore-<test suite name>
$(foreach suite,$(NETCORE_COREFX_TEST_SUITES),$(eval $(call NetcoreTestTemplate,netcore-$(suite),netcore-$(suite),$(suite),$(suite).dll,$($(suite)_PACKAGER_ARGS))))

$(foreach suite,$(NETCORE_COREFX_PASSING_TEST_SUITES),$(eval run-corefx-tests-netcore: run-$(suite)))

# Path to a fully built https://github.com/dotnet/performance
PERF_REPO_ROOT=

# Need to embed corelib because BenchmarkDotNet reads version info from it
build-bench: packager.exe $(NETCOREAPP_DIR) bench-runner/WasmRunner.cs
	@if test -z "$(PERF_REPO_ROOT)"; then echo "PERF_REPO_ROOT is not set"; exit 1; fi
	cd bench-runner && dotnet build /nologo /p:Configuration=Release
	$(PACKAGER) --debugrt --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/bench --nobinding --builddir=obj/bench --framework=netcoreapp3.0 --netcore-sdkdir=$(NETCOREAPP_DIR)/runtimes/webassembly-wasm/lib/netcoreapp3.0 --template=runtime-tests.js --search-path=bench-runner/bin/Release/netcoreapp3.0 --embed-file System.Private.CoreLib.dll@System.Private.CoreLib.dll --embed-file System.Text.RegularExpressions.dll --search-path=$(PERF_REPO_ROOT)/artifacts/bin/MicroBenchmarks/Release/netcoreapp5.0 --search-path=$(PERF_REPO_ROOT)/artifacts/bin/MicroBenchmarks/Release/netcoreapp5.0/runtimes/unix/lib/netstandard2.0 bench-runner/bin/Release/netcoreapp3.0/bench.dll MicroBenchmarks.dll
	ninja -v -C obj/bench

run-bench: build-bench
	(cd bin/bench && $(SM) runtime.js --enable-gc --setenv=MONO_LOG_MASK= --run bench.dll MicroBenchmarks.dll Perf_HashCode)

endif

runtime-threads:
	$(MAKE) -C ../builds package-wasm-runtime-threads

tuner:
	$(MAKE) -C ../../mcs/tools/wasm-tuner PROFILE=wasm_tools
	cp ../../mcs/class/lib/wasm_tools/wasm-tuner.exe* ../out/wasm-bcl/wasm_tools/wasm-tuner.exe*

cil-strip:
	$(MAKE) -C ../../mcs/tools/cil-strip  PROFILE=wasm_tools

mono: runtime runtime-threads cross bcl

BCL_DEPS=/r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.Core.dll /r:$(WASM_BCL_DIR)/System.dll /r:$(WASM_BCL_DIR)/System.Net.Http.dll /r:$(WASM_BCL_DIR)/Facades/netstandard.dll /r:$(WASM_BCL_DIR)/System.IO.Compression.dll /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll 

main.exe: $(APP_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -out:$@ $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(APP_SOURCES)

mini_tests.dll: $(MINI_TEST_SOURCES) mini-test-runner.cs
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ -define:__MOBILE__,ARCH_32 $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(MINI_TEST_SOURCES) mini-test-runner.cs

binding_tests.dll: $(WASM_FRAMEWORK)/.stamp-framework $(BINDING_TEST_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ $(WASM_FRAMEWORK_DEPS) $(BCL_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll /r:$(WASM_BCL_DIR)/nunitlite.dll $(BINDING_TEST_SOURCES)

$(WASM_FRAMEWORK)/.stamp-framework: $(WASM_FRAMEWORK_SOURCES)
	$(DOTNET_BUILD) $(WASM_FRAMEWORK_SOURCE)/WebAssembly.Framework.sln
	cp -f $(WASM_FRAMEWORK)/netstandard2.0/* $(WASM_FRAMEWORK)
	touch $@

Simple.Dependency.dll: dependency.cs $(WASM_FRAMEWORK)/.stamp-framework
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ $(BCL_DEPS) dependency.cs

sample.dll: Simple.Dependency.dll sample.cs $(WASM_FRAMEWORK)/.stamp-framework tests/debugger/*.cs
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:Simple.Dependency.dll /unsafe $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) sample.cs tests/debugger/*.cs

debugger-test.dll: tests/debugger/*.cs Simple.Dependency.dll
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:$(WASM_BCL_DIR)/mscorlib.dll /r:Simple.Dependency.dll /unsafe tests/debugger/*.cs

OPTIONS_CS = $(TOP)/mcs/class/Mono.Options/Mono.Options/Options.cs

$(TOP)/mcs/class/lib/wasm_tools/Mono.Cecil.dll: .stamp-bcl

Mono.Cecil.dll: $(TOP)/mcs/class/lib/wasm_tools/Mono.Cecil.dll
	cp $< $@

# Should use .csproj file and msbuild
packager.exe: packager.cs Mono.Cecil.dll $(OPTIONS_CS) | build-native
	$(CSC) $(CSC_FLAGS) /out:$@ /r:Mono.Cecil.dll packager.cs $(OPTIONS_CS) /r:$(API_REFS)/mscorlib.dll /r:$(API_REFS)/System.dll /r:$(API_REFS)/System.Core.dll

debug_sample/dotnet.js: packager.exe $(WASM_FRAMEWORK)/.stamp-framework sample.dll debug.html runtime.js
	$(PACKAGER) --copy=always -debug -out=debug_sample --asset=debug.html sample.dll
	touch $@

build-debug-sample: debug_sample/dotnet.js

TEST_ASSEMBLIES = $(WASM_BCL_DIR)/nunitlite.dll $(WASM_BCL_DIR)/tests/wasm_corlib_test.dll $(WASM_BCL_DIR)/tests/wasm_System_test.dll $(WASM_BCL_DIR)/tests/wasm_System.Core_test.dll

build-test-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework binding_tests.dll mini_tests.dll main.exe runtime-tests.js
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --template=runtime-tests.js --appdir=bin/test-suite --builddir=obj/test-suite --asset=tests/runtime/index.html binding_tests.dll mini_tests.dll main.exe $(TEST_ASSEMBLIES) --linker --no-linker-exclude-deserialization
	ninja -v -C obj/test-suite

XUNIT_DIR = $(TOP)/external/xunit-binaries

xunit-runner.exe: xunit-runner/xunit-runner.cs
	csc /out:$@ /r:$(XUNIT_DIR)/xunit.abstractions.dll /r:$(XUNIT_DIR)/xunit.core.dll /r:$(XUNIT_DIR)/xunit.console.exe /r:$(WASM_BCL_DIR)/Facades/System.Runtime.dll /r:$(WASM_BCL_DIR)/Facades/System.Reflection.dll /r:$(WASM_BCL_DIR)/Facades/System.Collections.dll /r:$(XUNIT_DIR)/xunit.execution.dotnet.dll /r:$(XUNIT_DIR)/xunit.runner.utility.net452.dll xunit-runner/xunit-runner.cs

#
# Targets for running xunit tests
# xunit-exclusions.rsp contains the exclusions
#

##
# Parameters:
# $(1) - make target suffix
# $(2) - directory name
# $(3) - test assembly filename
# $(4) - command line arguments
define XunitTestTemplate

bin/$(2)/dotnet.js: packager.exe $(WASM_FRAMEWORK)/.stamp-framework xunit-runner.exe runtime-tests.js xunit-exclusions.rsp
	$(PACKAGER) --debugrt --search-path=$(WASM_BCL_DIR)/tests --search-path=$(XUNIT_DIR) --template=runtime-tests.js --appdir=bin/$(2) --builddir=obj/$(2) --asset=xunit-exclusions.rsp xunit-runner.exe $(WASM_BCL_DIR)/tests/xunit.execution.dotnet.dll $(WASM_BCL_DIR)/tests/$(3) $(WASM_BCL_DIR)/tests/Xunit.NetCore.Extensions.dll
	ninja -v -C obj/$(2)

build-$(1): bin/$(2)/dotnet.js

run-$(1): bin/$(2)/dotnet.js
	(cd bin/$(2) && $$(SM) runtime.js --enable-gc --run xunit-runner.exe $(3) $(4) `cat xunit-exclusions.rsp | grep -v '#'`)

endef

$(eval $(call XunitTestTemplate,System-xunit,System-xunit,wasm_System_xunit-test.dll))
$(eval $(call XunitTestTemplate,Microsoft.CSharp-xunit,Microsoft.CSharp-xunit,wasm_Microsoft.CSharp_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Core-xunit,System.Core-xunit,wasm_System.Core_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Data-xunit,System.Data-xunit,wasm_System.Data_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.IO.Compression-xunit,System.IO.Compression-xunit,wasm_System.IO.Compression_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Net.Http.UnitTests-xunit,System.Net.Http.UnitTests-xunit,wasm_System.Net.Http.UnitTests_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Numerics-xunit,System.Numerics-xunit,wasm_System.Numerics_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Runtime.Serialization-xunit,System.Runtime.Serialization-xunit,wasm_System.Runtime.Serialization_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Security-xunit,System.Security-xunit,wasm_System.Security_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Xml-xunit,System.Xml-xunit,wasm_System.Xml_xunit-test.dll))
$(eval $(call XunitTestTemplate,System.Xml.Linq-xunit,System.Xml.Linq-xunit,wasm_System.Xml.Linq_xunit-test.dll))
$(eval $(call XunitTestTemplate,corlib-xunit,corlib-xunit,wasm_corlib_xunit-test.dll))

$(BROWSER_TEST)/.stamp-browser-test-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework $(BROWSER_TEST_SOURCES) clean-browser-tests build-sdk .stamp-npm-$(NPM_VERSION)
	$(DOTNET_BUILD) tests/browser/src -v normal
	(cd $(BROWSER_TEST) && $(NPM) install)
	touch $@

$(BROWSER_TEST_THREADS)/.stamp-browser-test-threads-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework $(BROWSER_TEST_SOURCES) clean-browser-tests build-sdk .stamp-npm-$(NPM_VERSION)
	$(DOTNET_BUILD) tests/browser/src -v normal -c DebugThreads
	(cd $(BROWSER_TEST) && $(NPM) install)
	touch $@

$(BROWSER_TEST_DYNAMIC)/.stamp-browser-test-dynamic-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework $(BROWSER_TEST_SOURCES) clean-browser-tests build-sdk  .stamp-npm-$(NPM_VERSION)
	echo BROWSER_TEST_SOURCES: $(BROWSER_TEST_SOURCES)
	$(DOTNET_BUILD) tests/browser/src -v normal -p:EnableMonoWasmDynamic=true
	(cd $(BROWSER_TEST) && $(NPM) install)
	touch $@

bin/debugger-test-suite/dotnet.wasm: packager.exe binding_tests.dll debugger-test.dll tests/debugger/debugger-driver.html other.js builds/debug/dotnet.js
	$(PACKAGER) --copy=always -debugrt -debug --template=runtime.js --template-output-name=runtime-debugger.js --builddir=obj/debugger-test-suite --appdir=bin/debugger-test-suite --asset=tests/debugger/debugger-driver.html --asset=other.js debugger-test.dll
	ninja -v -C obj/debugger-test-suite
	touch $@

build-aot-hello: packager.exe hello.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-hello --builddir=obj/aot-hello --aot --nobinding --template=runtime-tests.js --pinvoke-libs=libfoo hello.exe
	ninja -v -C obj/aot-hello

build-aot-hello-profiled: packager.exe hello.exe data.aotprofile
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-hello-profiled --nobinding --builddir=obj/aot-hello-profiled --aot --template=runtime-tests.js --pinvoke-libs=libfoo --aot-profile=data.aotprofile --aot-interp hello.exe
	ninja -v -C obj/aot-hello-profiled

$(WASM_TESTS_BIN):
	mkdir -p $@

$(WASM_TESTS_BIN)/zlibtest.exe: $(WASM_TESTS_SRC)/zlibtest.cs | $(WASM_TESTS_BIN)
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) $(WASM_TESTS_SRC)/zlibtest.cs

run-zlib-test: build-zlib-sample
	(cd bin/zlib-sample && $(SM) runtime.js --run zlibtest.exe)

build-zlib-sample: packager.exe $(WASM_TESTS_BIN)/zlibtest.exe
	$(PACKAGER) --nobinding --template=runtime-tests.js --out=bin/zlib-sample $(WASM_TESTS_BIN)/zlibtest.exe

build-zlib-interp-sample: packager.exe $(WASM_TESTS_BIN)/zlibtest.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/interp-zlib-sample --nobinding --builddir=obj/interp-zlib-sample --template=runtime-tests.js $(WASM_TESTS_BIN)/zlibtest.exe --linker --link-icalls --zlib
	ninja -v -C obj/interp-zlib-sample

run-zlib-interp-sample: build-zlib-interp-sample
	(cd bin/interp-zlib-sample && $(SM) runtime.js --run zlibtest.exe)

build-zlib-aot-hello: packager.exe $(WASM_TESTS_BIN)/zlibtest.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-zlib-sample --nobinding --builddir=obj/aot-zlib-sample --aot --template=runtime-tests.js $(WASM_TESTS_BIN)/zlibtest.exe  --zlib
	ninja -v -C obj/aot-zlib-sample

run-zlib-aot-hello: build-zlib-aot-hello
	(cd bin/aot-zlib-sample && $(SM) runtime.js --run zlibtest.exe)

# TimeZoneInfo tests

$(TEST_SOURCE_DIR)/tzdtest.exe: $(TEST_SOURCE_DIR)/tzdtest.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) $(TEST_SOURCE_DIR)/tzdtest.cs

run-tzd-sample: build-tzd-sample
	(cd bin/tzdtest-sample && $(SM) runtime.js --enable-zoneinfo --run tzdtest.exe)

build-tzd-sample: packager.exe $(TEST_SOURCE_DIR)/tzdtest.exe
	mono packager.exe --nobinding --template=runtime-tests.js --out=bin/tzdtest-sample --copy=ifnewer --asset=zoneinfo/zoneinfo.data --asset=zoneinfo/mono-webassembly-zoneinfo-fs-smd.js.metadata $(TEST_SOURCE_DIR)/tzdtest.exe
	cp -r /usr/share/zoneinfo ./bin/tzdtest-sample/zoneinfo

build-tzd-interp-sample: packager.exe $(TEST_SOURCE_DIR)/tzdtest.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out --copy=ifnewer --asset=zoneinfo/zoneinfo.data --asset=zoneinfo/mono-webassembly-zoneinfo-fs-smd.js.metadata -appdir=bin/interp-tzd-sample --enable-fs --nobinding --builddir=obj/interp-tzd-sample --template=runtime-tests.js $(TEST_SOURCE_DIR)/tzdtest.exe --linker --link-icalls
	ninja -v -C obj/interp-tzd-sample

run-tzd-interp-sample: build-tzd-interp-sample
	(cd bin/interp-tzd-sample && $(SM) runtime.js --enable-zoneinfo --run tzdtest.exe)

build-tzd-aot-sample: packager.exe $(TEST_SOURCE_DIR)/tzdtest.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-tzd-sample --enable-fs --nobinding --copy=ifnewer --asset=zoneinfo/zoneinfo.data --asset=zoneinfo/mono-webassembly-zoneinfo-fs-smd.js.metadata --builddir=obj/aot-tzd-sample --aot --template=runtime-tests.js $(TEST_SOURCE_DIR)/tzdtest.exe
	ninja -v -C obj/aot-tzd-sample

run-tzd-aot-sample: build-tzd-aot-sample
	(cd bin/aot-tzd-sample && $(SM) runtime.js --enable-zoneinfo --run tzdtest.exe)

# end TimeZoneInfo tests

build-mixed-sample: packager.exe hello.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --aot-interp --aot-assemblies=mscorlib --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/mixed-sample --nobinding --builddir=obj/mixed-sample --template=runtime-tests.js hello.exe
	ninja -v -C obj/mixed-sample

run-mixed-sample: build-mixed-sample
	(cd bin/mixed-sample && $(SM) runtime.js --run hello.exe)

hello.exe: hello.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) hello.cs

hello-io.exe: hello-io.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) hello-io.cs

run-aot-hello: build-aot-hello
	(cd bin/aot-hello && $(SM) runtime.js --run hello.exe)

run-aot-hello-profiled: build-aot-hello-profiled
	(cd bin/aot-hello-profiled && $(SM) runtime.js --run hello.exe)

build-interp-hello: packager.exe hello.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/interp-hello --nobinding --builddir=obj/interp-hello --template=runtime-tests.js hello.exe
	ninja -v -C obj/interp-hello

run-interp-hello: build-interp-hello
	(cd bin/interp-hello && $(SM) runtime.js --run hello.exe)

build-io-sample: packager.exe hello-io.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/io-sample --builddir=obj/io-sample --template=runtime.js --asset=hello-io.html --enable-fs hello-io.exe
	ninja -v -C obj/io-sample
	(cd bin/io-sample && $(PYTHON) $(EMSCRIPTEN_SDK_DIR)/upstream/emscripten/tools/file_packager.py mono.dat --js-output=mono-loader.js --preload ../../hello-io.cs@/)

threads.exe: threads.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) threads.cs

build-threads-sample: packager.exe threads.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/threads-sample --nobinding --builddir=obj/threads-sample --template=runtime-tests.js --asset=tests/runtime/index.html --threads threads.exe
	ninja -v -C obj/threads-sample

# (cd bin/threads-sample && $(D8) --experimental-wasm-threads --stack-trace-limit=1000 runtime.js -- --run threads.exe)
run-threads-sample: build-threads-sample
	(npm install --prefix tests/runtime/ && npm start --prefix tests/runtime)

build-aot-mini: packager.exe mini_tests.dll main.exe runtime.js
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-mini --nobinding --builddir=obj/aot-mini --aot --template=runtime-tests.js mini_tests.dll
	ninja -v -C obj/aot-mini

check-aot-mini run-aot-mini: build-aot-mini
	(cd bin/aot-mini && $(SM) runtime.js --regression mini_tests.dll)

get-sysnative-exports:
	nm ../out/wasm-runtime-release/lib/libmono-native.a  | grep SystemNative | cut -d ' ' -f 3 | grep -v '^$$'

build-aot-bindings-sample: packager.exe sample.dll
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-bindings-sample --builddir=obj/aot-bindings-sample --aot --template=runtime.js --link-mode=SdkOnly --asset=sample.html sample.dll
	ninja -v -C obj/aot-bindings-sample

build-aot-bindings-interp-sample: packager.exe sample.dll
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-bindings-interp-sample --builddir=obj/aot-bindings-interp-sample --aot --template=runtime.js --link-mode=SdkOnly --linker --link-icalls --asset=sample.html sample.dll
	ninja -v -C obj/aot-bindings-interp-sample

aot-profile/main.exe: aot-profile/main.cs
	$(CSC) $(CSC_FLAGS) /unsafe -out:$@ $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll aot-profile/main.cs

.PHONY: build-aot-profile
build-aot-profile: packager.exe aot-profile/main.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-profile --builddir=obj/aot-profile --profile=aot --template=aot-profile/runtime.js --asset=aot-profile/index.html aot-profile/main.exe
	ninja -C obj/aot-profile

data.aotprofile: build-aot-profile
	node aot-profile/gen-profile.js $@ bin/aot-profile

coverage-profile/main.exe: coverage-profile/main.cs
	$(CSC) $(CSC_FLAGS) /unsafe -out:$@ $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll coverage-profile/main.cs

.PHONY:
build-coverage: packager.exe coverage-profile/main.exe
	$(PACKAGER) --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/coverage-profile --builddir=obj/coverage-profile --profile=coverage --template=coverage-profile/runtime.js --asset=coverage-profile/index.html coverage-profile/main.exe
	ninja -C obj/coverage-profile

coverage.xml: build-coverage
	node coverage-profile/gen-profile.js $@ bin/coverage-profile

##
# Parameters:
# $(1) - make target suffix
#
define InterpTestTemplate
build-interp-$(1): packager.exe $(WASM_FRAMEWORK)/.stamp-framework binding_tests.dll mini_tests.dll main.exe runtime-tests.js
	$(PACKAGER) --template=runtime-tests.js --appdir=bin/interp-$(1) --builddir=obj/interp-$(1) --asset=tests/runtime/index.html binding_tests.dll mini_tests.dll main.exe $(TEST_ASSEMBLIES) --linker
	ninja -v -C obj/interp-$(1)

clean-interp-$(1):
	$(RM) -r obj/interp-$(1) bin/interp-$(1)
endef

$(eval $(call InterpTestTemplate,mini))
$(eval $(call InterpTestTemplate,corlib))
$(eval $(call InterpTestTemplate,System.Core))
$(eval $(call InterpTestTemplate,System))

##
# Parameters:
# $(1) - make target suffix
# $(2) - test assembly filename
# $(3) - main.exe argument
# $(4) - extra args to packager
define AotTestTemplate

build-aot-$(1): packager.exe runtime-tests.js main.exe $$(WASM_FRAMEWORK)/.stamp-framework $$(WASM_FRAMEWORK)/WebAssembly.Bindings.dll binding_tests.dll $$(WASM_BCL_DIR)/nunitlite.dll $$(WASM_BCL_DIR)/tests/$(2)
	$(PACKAGER) --emscripten-sdkdir=$$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$$(TOP)/sdks/out -appdir=bin/aot-$(1) --builddir=obj/aot-$(1) --aot --template=runtime-tests.js main.exe binding_tests.dll $$(WASM_BCL_DIR)/tests/$(2) --linker --link-icalls $(4)
	ninja -v -C obj/aot-$(1)

clean-aot-$(1):
	$(RM) -r obj/aot-$(1) bin/aot-$(1)

check-aot-$(1) run-aot-$(1): build-aot-$(1)
	(cd bin/aot-$(1) && $$(SM) runtime.js --exclude StackWalks,StaticAotNotWorking,StaticLinkedAotNotWorking,SRE $(3))

endef

$(eval $(call AotTestTemplate,corlib,wasm_corlib_test.dll,corlib))
$(eval $(call AotTestTemplate,System.Core,wasm_System.Core_test.dll,System.Core))
$(eval $(call AotTestTemplate,System,wasm_System_test.dll,System))

$(eval $(call AotTestTemplate,mixed-corlib,wasm_corlib_test.dll,corlib,--aot-interp --aot-assemblies=mscorlib))
$(eval $(call AotTestTemplate,mixed-System.Core,wasm_System.Core_test.dll,System.Core,--aot-interp --aot-assemblies=mscorlib))

AOT_TEST_SUITES=corlib System System.Core

build-aot-all: build
	$(MAKE) $(patsubst %,build-aot-%,$(AOT_TEST_SUITES))

run-aot-all: build-aot-all
	for suite in $(AOT_TEST_SUITES); do $(MAKE) run-aot-$$suite || exit 1; done

rebuild-debug-sample: clean-debug-sample build-debug-sample

build-debugger-test-app: bin/debugger-test-suite/dotnet.wasm
build-managed: build-debug-sample build-test-suite

build-dbg-proxy:
	$(DOTNET_BUILD) BrowserDebugHost

build-dbg-testsuite:
	$(DOTNET_BUILD) DebuggerTestSuite

build: build-native build-managed build-debugger-test-app

gen-pinvoke-tables:
	mono ../../mcs/class/lib/wasm_tools/wasm-tuner.exe --gen-pinvoke-table System.Native $(WASM_BCL_DIR)/mscorlib.dll > src/pinvoke-tables-default.h

D8=~/.jsvu/v8
JSC=~/.jsvu/jsc
SM=~/.jsvu/sm

RUN_V8=$(D8) --stack-trace-limit=1000 --expose_wasm

run-v8-%: toolchain build-test-suite
	(cd bin/test-suite && $(D8) --stack-trace-limit=1000 --expose_wasm runtime.js -- --enable-gc $*)

run-jsc-%: toolchain build-test-suite
	(cd bin/test-suite && $(JSC) runtime.js -- $*)

run-sm-%: toolchain build-test-suite
	(cd bin/test-suite && $(SM) runtime.js $*)

run-chrome-%: build-test-suite
	node tests/runtime/run.js bin/test-suite $*

# Leaving JSC for now cuz it aborts when it encounters wasm
run-all-%:
	$(MAKE) -C . run-v8-$*
	$(MAKE) -C . run-sm-$*
	$(MAKE) -C . run-jsc-$*

run-debugger-tests: build-debugger-test-app build-dbg-testsuite
	if [ ! -z "$(TEST_FILTER)" ]; then \
		LC_ALL=en_US.UTF-8 dotnet test DebuggerTestSuite --filter FullyQualifiedName~$(TEST_FILTER); \
	else \
		LC_ALL=en_US.UTF-8 dotnet test DebuggerTestSuite $(TEST_ARGS); \
	fi

run-browser-tests: build-browser-test-suite
	(cd $(BROWSER_TEST) && $(NPM) test)

build-browser-test-suite: $(BROWSER_TEST)/.stamp-browser-test-suite

run-browser-threads-tests: $(BROWSER_TEST_THREADS)/.stamp-browser-test-threads-suite
	(cd $(BROWSER_TEST_THREADS) && $(NPM) test)

run-browser-dynamic-tests: $(BROWSER_TEST_DYNAMIC)/.stamp-browser-test-dynamic-suite
	(cd $(BROWSER_TEST_DYNAMIC) && $(NPM) test)

run-browser-safari-tests: $(BROWSER_TEST)/.stamp-browser-test-suite
	(cd $(BROWSER_TEST) && $(NPM) run testsafari)

clean: clean-browser-tests
	$(RM) -r *.o
	$(RM) -r ./obj
	$(RM) -r ./bin
	$(RM) -r debug_sample/dotnet.js
	$(RM) -r .stamp-build-test-suite
	$(RM) -r sample.dll
	$(RM) -r Simple.Dependency.dll
	$(RM) -r packager.exe
	$(RM) -r mini_tests.dll
	$(RM) -r main.exe
	$(RM) -r binding-tests.dll
	$(RM) -r coverage.xml
	$(RM) -r $(WASM_FRAMEWORK)/.stamp-framework
	$(RM) -r $(WASM_FRAMEWORK)/netstandard2.0
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.dll
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.pdb
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.xml
	$(RM) -r $(NETCOREAPP_DIR)
	$(RM) -r builds
	$(RM) emsdk_env.sh
	$(RM) .configured
	$(RM) src/pinvoke-tables-default-netcore.h

package: build build-sdk build-dbg-proxy
	rm -rf tmp
	mkdir tmp
	mkdir tmp/wasm-bcl
	mkdir tmp/wasm-bcl/wasm
	mkdir tmp/wasm-bcl/wasm_tools
	cp -r $(TOP)/sdks/out/wasm-bcl/wasm/* tmp/wasm-bcl/wasm/
	cp -r $(TOP)/sdks/out/wasm-bcl/wasm_tools/* tmp/wasm-bcl/wasm_tools/
	rm -rf tmp/wasm-bcl/wasm/bare
	rm -rf tmp/wasm-bcl/wasm/secxml
	rm -rf tmp/wasm-bcl/wasm/tests
	rm -rf tmp/wasm-bcl/wasm/corlib.unsafe.dll.tmp
	cp -r src/ tmp/src
	cp $(MONO_LIBS) tmp/
	cp -r builds tmp/
	mkdir tmp/framework
	cp framework/WebAssembly.Bindings.dll tmp/framework
	cp framework/WebAssembly.Bindings.pdb tmp/framework
	cp framework/WebAssembly.Bindings.xml tmp/framework
	cp framework/System.Net.Http.WebAssemblyHttpHandler.dll tmp/framework
	cp framework/System.Net.Http.WebAssemblyHttpHandler.pdb tmp/framework
	cp framework/System.Net.Http.WebAssemblyHttpHandler.xml tmp/framework
	cp framework/WebAssembly.Net.WebSockets.dll tmp/framework
	cp framework/WebAssembly.Net.WebSockets.pdb tmp/framework
	cp framework/WebAssembly.Net.WebSockets.xml tmp/framework
	mkdir tmp/framework/src
	cp -r framework/src tmp/framework
	$(RM) -r tmp/framework/src/**/bin
	$(RM) -r tmp/framework/src/**/obj
	cp sample.html tmp/
	cp sample.cs tmp/
	cp dependency.cs tmp/
	cp README.md tmp/
	cp server.py tmp/
	cp Mono.Cecil.dll tmp/
	cp $(OPTIONS_CS) tmp/	
	cp packager.exe tmp/
	cp runtime.js tmp/
	cp BrowserDebugProxy/bin/Debug/netcoreapp3.0/BrowserDebugProxy.dll tmp/
	cp BrowserDebugProxy/bin/Debug/netcoreapp3.0/BrowserDebugProxy.pdb tmp/
	mkdir tmp/dbg-proxy
	cp -r BrowserDebugHost/bin/Debug/netcoreapp3.0/ tmp/dbg-proxy/
	mkdir tmp/docs
	cp -r ./docs/ tmp/docs/
	mkdir tmp/packages
	cp -r ./sdk/packages tmp/
	mkdir tmp/Templates
	cp -r ./sdk/Templates tmp/
	mkdir tmp/support
	(cd tmp;  zip -r9 ../mono-wasm-$(shell git rev-parse --short HEAD).zip .)
	rm -rf tmp

serve:
	$(PYTHON) server.py

canary:
	/Applications/Google\ Chrome\ Canary.app/Contents/MacOS/Google\ Chrome\ Canary --remote-debugging-port=9222

check-aot: do-aot-hello

clean-debug-sample:
	$(RM) debug_sample/dotnet.js
	$(RM) .stamp-build-debugger-test-app

clean-sdk:
	$(RM) -r sdk/**/bin
	$(RM) -r sdk/**/obj
	$(RM) -r sdk/**/**/bin
	$(RM) -r sdk/**/**/obj
	$(RM) -r sdk/packages
	$(RM) -r $(WASM_FRAMEWORK)/.stamp-framework

build-sdk: $(WASM_FRAMEWORK)/.stamp-framework
	$(DOTNET_BUILD) sdk/MonoWasmSdkCLI.sln
	$(DOTNET_PACK) sdk/Templates/Mono.WebAssembly.Templates.csproj

clean-browser-tests: clean-sdk
	$(RM) -r $(BROWSER_TEST)/.stamp-browser-test-suite
	$(RM) -r $(BROWSER_TEST)/.stamp-browser-test-threads-suite
	$(RM) -r $(BROWSER_TEST)/node-modules
	$(RM) -r $(BROWSER_TEST)/publish
	$(RM) -r $(BROWSER_TEST)/src/**/bin $(BROWSER_TEST)/src/**/**/bin $(BROWSER_TEST)/src/**/obj $(BROWSER_TEST)/src/**/**/obj publish

clean-framework:
	$(RM) -r $(WASM_FRAMEWORK)/.stamp-framework
	$(RM) -r $(WASM_FRAMEWORK)/netstandard2.0
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.dll
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.pdb
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.xml
