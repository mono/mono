TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
-include $(TOP)/mcs/build/config.make
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

all: build

MAKEFLAGS += --no-builtin-rules
#emcc has lots of bash'isms
SHELL:=/bin/bash

#Use either 'release' or 'debug' depending on what you need
DRIVER_CONF=release

CSC_LOCATION?=$(STANDALONE_CSC_LOCATION)

EMSCRIPTEN_SDK_DIR?=$(TOP)/sdks/builds/toolchains/emsdk
EMCC=source $(CURDIR)/emsdk_env.sh && emcc
EMCC_DYNAMIC=EMCC_FORCE_STDLIBS=1 $(EMCC)
WASM_BCL_DIR=$(TOP)/sdks/out/wasm-bcl/wasm
WASM_RUNTIME_DIR=$(TOP)/sdks/out/wasm-runtime-release
MINI_PATH=$(TOP)/mono/mini
CSC?= MONO_PATH=$(TOP)/mcs/class/lib/build $(TOP)/sdks/builds/bcl/runtime/mono-wrapper $(CSC_LOCATION)
CSC_FLAGS := /debug:portable /noconfig /nostdlib /nologo /langversion:latest
API_REFS=$(TOP)/external/binary-reference-assemblies/v4.6
MONO_SUPPORT=$(WASM_RUNTIME_DIR)/include/support

ifeq ($(HOST_PLATFORM),win32)
DOTNET_CMD := $(shell powershell -ExecutionPolicy Bypass -Command "./init-tools.ps1")/dotnet.exe
DOTNET_CMD := "$(subst \,/,$(DOTNET))"
else
DOTNET_CMD := $(shell ./init-tools.sh | tail -1)
endif

DOTNET_PACK=$(DOTNET_CMD) pack
DOTNET_BUILD=$(DOTNET_CMD) build

ZLIB_HEADERS = \
	$(MONO_SUPPORT)/crc32.h		\
	$(MONO_SUPPORT)/deflate.h  	\
	$(MONO_SUPPORT)/inffast.h  	\
	$(MONO_SUPPORT)/inffixed.h  	\
	$(MONO_SUPPORT)/inflate.h  	\
	$(MONO_SUPPORT)/inftrees.h  	\
	$(MONO_SUPPORT)/trees.h  	\
	$(MONO_SUPPORT)/zconf.h  	\
	$(MONO_SUPPORT)/zlib.h  	\
	$(MONO_SUPPORT)/zutil.h

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
WASM_FRAMEWORK_DEPS=/r:$(WASM_FRAMEWORK)/WebAssembly.Bindings.dll /r:$(WASM_FRAMEWORK)/WebAssembly.Net.Http.dll /r:$(WASM_FRAMEWORK)/WebAssembly.Net.WebSockets.dll

BROWSER_TEST=$(TOP)/sdks/wasm/tests/browser
BROWSER_TEST_THREADS=$(TOP)/sdks/wasm/tests/browser
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

UNAME := $(shell uname -s)
ifeq ($(UNAME),Linux)
	JSVU_OS=linux64
endif
ifeq ($(UNAME),Darwin)
	JSVU_OS=mac64
endif
.PHONY: jsup

jsup:
	node node_modules/jsvu/cli.js --os=$(JSVU_OS) --engines=all

.stamp-jsvu:
	npm install
	node node_modules/jsvu/cli.js --os=$(JSVU_OS) --engines=all
	touch $@

.PHONY: toolchain
toolchain: .stamp-jsvu

# emsdk_env.sh calls emsdk construct_env which is a bit slow so make a copy
emsdk_env.sh:
	cd $(EMSCRIPTEN_SDK_DIR) && ./emsdk construct_env $(CURDIR)/emsdk_env.sh

MONO_LIBS = $(TOP)/sdks/out/wasm-runtime-release/lib/{libmono-ee-interp.a,libmono-native.a,libmono-icall-table.a,libmonosgen-2.0.a,libmono-ilgen.a}
MONO_DYNAMIC_LIBS = $(TOP)/sdks/out/wasm-runtime-dynamic-release/lib/{libmono-ee-interp.a,libmono-native.a,libmono-icall-table.a,libmonosgen-2.0.a,libmono-ilgen.a}

EMCC_FLAGS=-s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString', 'addFunction']" -s USE_ZLIB=1 -s "EXPORTED_FUNCTIONS=['_putchar']" --source-map-base http://example.com  -s WASM_OBJECT_FILES=0
EMCC_DEBUG_FLAGS =-g4 -Os -s ASSERTIONS=1
EMCC_RELEASE_FLAGS=-Oz --llvm-opts 2 --llvm-lto 1
EMCC_RELEASE_DYNAMIC_FLAGS=$(EMCC_RELEASE_FLAGS) -s MAIN_MODULE=2 -s EXPORT_ALL=1 -s DISABLE_EXCEPTION_CATCHING=0 -s ALLOW_TABLE_GROWTH=1 -s USE_ZLIB=0 -s WASM_OBJECT_FILES=0 -DWASM_SUPPORTS_DLOPEN
EMCC_THREADS_FLAGS=-s ALLOW_MEMORY_GROWTH=0 -s USE_PTHREADS=1 -s TOTAL_MEMORY=536870912 -pthread -Wl,--shared-memory,--no-check-features -s PTHREAD_POOL_SIZE=2

#
# Interpreter builds
#

MONO_LIBS = $(TOP)/sdks/out/wasm-runtime-release/lib/{libmono-ee-interp.a,libmono-native.a,libmonosgen-2.0.a,libmono-ilgen.a,libmono-icall-table.a}
MONO_NETCORE_LIBS=$(TOP)/sdks/out/wasm-runtime-netcore-release/lib/{libmono-ee-interp.a,libmonosgen-2.0.a,libmono-ilgen.a,libmono-icall-table.a} netcore-System.Native.bc
NETCORE_CFLAGS=-DENABLE_NETCORE

# $(1) - name
# $(2) - runtime dir
# $(3) - EMCC_FLAGS
# $(4) - libs
define InterpBuildTemplate

builds/$(1)/:
	mkdir -p $$@

builds/$(1)/mono.js: builds/$(1)/driver.o builds/$(1)/corebindings.o builds/$(1)/zlib-helper.o src/library_mono.js src/binding_support.js src/dotnet_support.js $(TOP)/sdks/out/$(2)/lib/libmonosgen-2.0.a | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(EMCC_FLAGS) $(3) --js-library src/library_mono.js --js-library src/binding_support.js --js-library src/dotnet_support.js builds/$(1)/driver.o builds/$(1)/corebindings.o builds/$(1)/zlib-helper.o $(4) -o builds/$(1)/mono.js

builds/$(1)/driver.o: src/driver.c src/corebindings.c src/pinvoke-tables-default.h | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(EMCC_FLAGS) $(3) -Oz -DCORE_BINDINGS -I$(WASM_RUNTIME_DIR)/include/mono-2.0 src/driver.c -c -o $$@

builds/$(1)/corebindings.o: src/corebindings.c | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(3) -Oz -I$(WASM_RUNTIME_DIR)/include/mono-2.0 src/corebindings.c -c -o $$@

builds/$(1)/zlib-helper.o: src/zlib-helper.c | builds/$(1)/ emsdk_env.sh
	$(EMCC) $(3) -Oz -I$(WASM_RUNTIME_DIR)/include/mono-2.0 -I$(WASM_RUNTIME_DIR)/include/support src/zlib-helper.c -c -o $$@

build-native: builds/$(1)/mono.js

endef

$(eval $(call InterpBuildTemplate,debug,wasm-runtime-release,$(EMCC_DEBUG_FLAGS),$(MONO_LIBS)))
$(eval $(call InterpBuildTemplate,release,wasm-runtime-release,$(EMCC_RELEASE_FLAGS),$(MONO_LIBS)))
ifdef ENABLE_WASM_THREADS
$(eval $(call InterpBuildTemplate,threads-debug,wasm-runtime-threads-release,$(EMCC_DEBUG_FLAGS) $(EMCC_THREADS_FLAGS),$(MONO_LIBS)))
$(eval $(call InterpBuildTemplate,threads-release,wasm-runtime-threads-release,$(EMCC_RELEASE_FLAGS) $(EMCC_THREADS_FLAGS),$(MONO_LIBS)))
endif
ifdef ENABLE_WASM_DYNAMIC_RUNTIME
$(eval $(call InterpBuildTemplate,release-dynamic,wasm-runtime-dynamic-release,$(EMCC_RELEASE_DYNAMIC_FLAGS),$(MONO_DYNAMIC_LIBS)))
endif
ifdef ENABLE_WASM_NETCORE
$(eval $(call InterpBuildTemplate,netcore-debug,wasm-runtime-netcore-release,$(EMCC_DEBUG_FLAGS) $(NETCORE_CFLAGS),$(MONO_NETCORE_LIBS)))
$(eval $(call InterpBuildTemplate,netcore-release,wasm-runtime-netcore-release,$(EMCC_RELEASE_FLAGS) $(NETCORE_CFLAGS),$(MONO_NETCORE_LIBS)))

builds/netcore-debug/mono.js builds/netcore-release/mono.js: netcore-System.Native.bc
endif

runtime:
	$(MAKE) -C ../builds package-wasm-runtime

cross:
	$(MAKE) -C ../builds package-wasm-cross

bcl:
	$(MAKE) -C ../builds package-wasm-bcl

ifdef ENABLE_WASM_NETCORE

NETCOREAPP_VERSION := $(shell cat ../../eng/Versions.props | sed -n 's/.*MicrosoftNETCoreAppVersion>\(.*\)<\/MicrosoftNETCoreAppVersion.*/\1/p' )
NETCOREAPP_DIR=../../netcore/shared/Microsoft.NETCore.App/$(NETCOREAPP_VERSION)

.PHONY: runtime-netcore
runtime-netcore:
	$(MAKE) -C ../builds package-wasm-runtime-netcore

.PHONY: bcl-netcore
bcl-netcore:
	$(MAKE) -C ../builds package-wasm-bcl-netcore

build-managed: bcl-netcore

# Set COREFX_ROOT to a compatible corefx source repo
# Apply netcore-corefx.diff
netcore-System.Native.bc:
	@if test x$(COREFX_ROOT) == x; then echo "Set COREFX_ROOT."; exit 1; fi
	cd $(COREFX_ROOT)/src/Native && touch _version.c && cd Unix && mkdir -p build && cd build && source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && emcmake cmake "-DEMSCRIPTEN_GENERATE_BITCODE_STATIC_LIBRARIES=1" "-DCMAKE_TOOLCHAIN_FILE=$(TOP)/sdks/builds/toolchains/emsdk/upstream/emscripten/cmake/Modules/Platform/Emscripten.cmake" -G Ninja -DCMAKE_BUILD_TYPE=Debug .. && ninja && cp System.Native/System.Native.bc $(PWD)/$@

src/pinvoke-tables-default-netcore.h:
	mono ../../mcs/class/lib/wasm_tools/wasm-tuner.exe --gen-pinvoke-table System.Native $(NETCOREAPP_DIR)/System.Private.CoreLib.dll $(NETCOREAPP_DIR)/System.Console.dll > $@

samples/hello/bin/Debug/netcoreapp3.0/hello.dll:
	netcore build samples/hello -f netcoreapp3.0

build-hello-netcore: packager.exe samples/hello/bin/Debug/netcoreapp3.0/hello.dll
	mono --debug packager.exe --debugrt --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/hello-netcore --nobinding --builddir=obj/hello-netcore --framework=netcoreapp3.0 --netcore-sdkdir=$(NETCOREAPP_DIR) --template=runtime-tests.js --linker samples/hello/bin/Debug/netcoreapp3.0/hello.dll
	ninja -v -C obj/hello-netcore

bin/hello-netcore/bin/mono.js: build-hello-netcore

run-hello-netcore: bin/hello-netcore/bin/mono.js
	(cd bin/hello-netcore && $(SM) runtime.js --run hello.dll)

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

sample.dll: Simple.Dependency.dll sample.cs $(WASM_FRAMEWORK)/.stamp-framework
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:Simple.Dependency.dll $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) sample.cs

debugger-test.dll: debugger-test.cs debugger-test2.cs Simple.Dependency.dll
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:$(WASM_BCL_DIR)/mscorlib.dll /r:Simple.Dependency.dll debugger-test.cs debugger-test2.cs

OPTIONS_CS = $(TOP)/mcs/class/Mono.Options/Mono.Options/Options.cs

Mono.Cecil.dll: $(TOP)/mcs/class/lib/wasm_tools/Mono.Cecil.dll
	cp $< $@

# Should use .csproj file and msbuild
packager.exe: packager.cs Mono.Cecil.dll $(OPTIONS_CS) | build-native
	$(CSC) $(CSC_FLAGS) /out:$@ /r:Mono.Cecil.dll packager.cs $(OPTIONS_CS) /r:$(API_REFS)/mscorlib.dll /r:$(API_REFS)/System.dll /r:$(API_REFS)/System.Core.dll

.stamp-build-debug-sample: packager.exe $(WASM_FRAMEWORK)/.stamp-framework sample.dll debug.html runtime.js
	mono --debug packager.exe --copy=always -debug -out=debug_sample --asset=debug.html sample.dll
	touch $@	

TEST_ASSEMBLIES = $(WASM_BCL_DIR)/nunitlite.dll $(WASM_BCL_DIR)/tests/wasm_corlib_test.dll $(WASM_BCL_DIR)/tests/wasm_System_test.dll $(WASM_BCL_DIR)/tests/wasm_System.Core_test.dll

build-test-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework binding_tests.dll mini_tests.dll main.exe runtime-tests.js
	mono packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --template=runtime-tests.js --appdir=bin/test-suite --builddir=obj/test-suite --asset=tests/runtime/index.html binding_tests.dll mini_tests.dll main.exe $(TEST_ASSEMBLIES) --linker
	ninja -v -C obj/test-suite

XUNIT_DIR = $(TOP)/external/xunit-binaries

xunit-runner.exe: xunit-runner.cs
	csc /out:$@ /r:$(XUNIT_DIR)/xunit.abstractions.dll /r:$(XUNIT_DIR)/xunit.core.dll /r:$(XUNIT_DIR)/xunit.console.exe /r:$(WASM_BCL_DIR)/Facades/System.Runtime.dll /r:$(WASM_BCL_DIR)/Facades/System.Reflection.dll /r:$(WASM_BCL_DIR)/Facades/System.Collections.dll /r:$(XUNIT_DIR)/xunit.execution.dotnet.dll /r:$(XUNIT_DIR)/xunit.runner.utility.net452.dll xunit-runner.cs

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

bin/$(2)/mono.js: packager.exe $(WASM_FRAMEWORK)/.stamp-framework xunit-runner.exe runtime-tests.js xunit-exclusions.rsp
	mono packager.exe --debugrt --search-path=$(XUNIT_DIR) --search-path=$(WASM_BCL_DIR)/tests --template=runtime-tests.js --appdir=bin/$(2) --builddir=obj/$(2) --asset=xunit-exclusions.rsp xunit-runner.exe $(XUNIT_DIR)/xunit.execution.dotnet.dll $(WASM_BCL_DIR)/tests/$(3) $(WASM_BCL_DIR)/tests/Xunit.NetCore.Extensions.dll
	ninja -v -C obj/$(2)

build-$(1): bin/$(2)/mono.js

run-$(1): bin/$(2)/mono.js
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

$(BROWSER_TEST)/.stamp-browser-test-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework $(BROWSER_TEST_SOURCES) clean-browser-tests build-sdk 
	$(DOTNET_BUILD) tests/browser/src -v normal
	(cd $(BROWSER_TEST) && npm install)
	touch $@

$(BROWSER_TEST_THREADS)/.stamp-browser-test-threads-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework $(BROWSER_TEST_SOURCES) clean-browser-tests build-sdk
	$(DOTNET_BUILD) tests/browser/src -v normal -c DebugThreads
	(cd $(BROWSER_TEST) && npm install)
	touch $@

.stamp-build-debugger-test-app: packager.exe binding_tests.dll debugger-test.dll debugger-driver.html
	mono packager.exe --copy=always -debugrt -debug --template=runtime.js --builddir=obj/debugger-test-suite --appdir=bin/debugger-test-suite --asset=debugger-driver.html debugger-test.dll
	ninja -v -C obj/debugger-test-suite
	touch $@

build-aot-sample: packager.exe hello.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-sample --nobinding --builddir=obj/aot-sample --aot --template=runtime-tests.js --pinvoke-libs=libfoo hello.exe
	ninja -v -C obj/aot-sample

build-aot-sample-profiled: packager.exe hello.exe data.aotprofile
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-sample-profiled --nobinding --builddir=obj/aot-sample-profiled --aot --template=runtime-tests.js --pinvoke-libs=libfoo --aot-profile=data.aotprofile --aot-interp hello.exe
	ninja -v -C obj/aot-sample-profiled

zlibtest.exe: zlibtest.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) zlibtest.cs

run-zlib-test: build-zlib-sample
	(cd bin/zlib-sample && $(SM) runtime.js --run zlibtest.exe)

build-zlib-sample: packager.exe zlibtest.exe
	mono packager.exe --nobinding --template=runtime-tests.js --out=bin/zlib-sample zlibtest.exe

build-zlib-interp-sample: packager.exe zlibtest.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/interp-zlib-sample --nobinding --builddir=obj/interp-zlib-sample --template=runtime-tests.js zlibtest.exe --linker --link-icalls --zlib
	ninja -v -C obj/interp-zlib-sample

run-zlib-interp-sample: build-zlib-interp-sample
	(cd bin/interp-zlib-sample && $(SM) runtime.js --run zlibtest.exe)

build-zlib-aot-sample: packager.exe zlibtest.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-zlib-sample --nobinding --builddir=obj/aot-zlib-sample --aot --template=runtime-tests.js zlibtest.exe  --zlib
	ninja -v -C obj/aot-zlib-sample

run-zlib-aot-sample: build-zlib-aot-sample
	(cd bin/aot-zlib-sample && $(SM) runtime.js --run zlibtest.exe)

build-mixed-sample: packager.exe hello.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --aot-interp --aot-assemblies=mscorlib --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/mixed-sample --nobinding --builddir=obj/mixed-sample --template=runtime-tests.js hello.exe
	ninja -v -C obj/mixed-sample

run-mixed-sample: build-mixed-sample
	(cd bin/mixed-sample && $(SM) runtime.js --run hello.exe)

hello.exe: hello.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) hello.cs

run-aot-sample: build-aot-sample
	(cd bin/aot-sample && $(SM) runtime.js --run hello.exe)

run-aot-sample-profiled: build-aot-sample-profiled
	(cd bin/aot-sample-profiled && $(SM) runtime.js --run hello.exe)

build-interp-sample: packager.exe hello.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/interp-sample --nobinding --builddir=obj/interp-sample --template=runtime-tests.js hello.exe --linker --link-icalls
	ninja -v -C obj/interp-sample

run-interp-sample: build-interp-sample
	(cd bin/interp-sample && $(SM) runtime.js --run hello.exe)

threads.exe: threads.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) threads.cs

build-threads-sample: packager.exe threads.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/threads-sample --nobinding --builddir=obj/threads-sample --template=runtime-tests.js --asset=tests/runtime/index.html --threads threads.exe
	ninja -v -C obj/threads-sample

# (cd bin/threads-sample && $(D8) --experimental-wasm-threads --stack-trace-limit=1000 runtime.js -- --run threads.exe)
run-threads-sample: build-threads-sample
	(npm install --prefix tests/runtime/ && npm start --prefix tests/runtime)

build-aot-mini: packager.exe mini_tests.dll main.exe runtime.js
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-mini --nobinding --builddir=obj/aot-mini --aot --template=runtime-tests.js mini_tests.dll
	ninja -v -C obj/aot-mini

check-aot-mini run-aot-mini: build-aot-mini
	(cd bin/aot-mini && $(SM) runtime.js --regression mini_tests.dll)

get-sysnative-exports:
	nm ../out/wasm-runtime-release/lib/libmono-native.a  | grep SystemNative | cut -d ' ' -f 3 | grep -v '^$$'

build-aot-bindings-sample: packager.exe sample.dll
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-bindings-sample --builddir=obj/aot-bindings-sample --aot --template=runtime.js --link-mode=SdkOnly --asset=sample.html sample.dll
	ninja -v -C obj/aot-bindings-sample

build-aot-bindings-interp-sample: packager.exe sample.dll
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-bindings-interp-sample --builddir=obj/aot-bindings-interp-sample --aot --template=runtime.js --link-mode=SdkOnly --linker --link-icalls --asset=sample.html sample.dll
	ninja -v -C obj/aot-bindings-interp-sample

aot-profile/main.exe: aot-profile/main.cs
	$(CSC) $(CSC_FLAGS) /unsafe -out:$@ $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll aot-profile/main.cs

.PHONY: build-aot-profile
build-aot-profile: packager.exe aot-profile/main.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-profile --builddir=obj/aot-profile --profile=aot --template=aot-profile/runtime.js --asset=aot-profile/index.html aot-profile/main.exe
	ninja -C obj/aot-profile

data.aotprofile: build-aot-profile
	node aot-profile/gen-profile.js $@ bin/aot-profile

##
# Parameters:
# $(1) - make target suffix
#
define InterpTestTemplate
build-interp-$(1): packager.exe $(WASM_FRAMEWORK)/.stamp-framework binding_tests.dll mini_tests.dll main.exe runtime-tests.js
	mono packager.exe --template=runtime-tests.js --appdir=bin/interp-$(1) --builddir=obj/interp-$(1) --asset=tests/runtime/index.html binding_tests.dll mini_tests.dll main.exe $(TEST_ASSEMBLIES) --linker
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
	mono --debug packager.exe --emscripten-sdkdir=$$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$$(TOP)/sdks/out -appdir=bin/aot-$(1) --builddir=obj/aot-$(1) --aot --template=runtime-tests.js main.exe binding_tests.dll $$(WASM_BCL_DIR)/tests/$(2) --linker --link-icalls $(4)
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

build-debug-sample: .stamp-build-debug-sample

build-debugger-test-app: .stamp-build-debugger-test-app
build-managed: build-debug-sample build-test-suite

build-dbg-proxy:
	$(DOTNET_BUILD) ProxyDriver

build-dbg-testsuite:
	$(DOTNET_BUILD) DebuggerTestSuite

build: build-native build-managed build-debugger-test-app

gen-pinvoke-tables:
	mono ../../mcs/class/lib/wasm_tools/wasm-tuner.exe --gen-pinvoke-table System.Native $(WASM_BCL_DIR)/mscorlib.dll > src/pinvoke-tables-default.h

CHAKRA=~/.jsvu/ch
D8=~/.jsvu/v8
JSC=~/.jsvu/jsc
SM=~/.jsvu/sm

run-ch-%: toolchain build-test-suite
	(cd bin/test-suite && $(CHAKRA) runtime.js -args $*)

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
	$(MAKE) -C . run-ch-$*
	$(MAKE) -C . run-v8-$*
	$(MAKE) -C . run-sm-$*
	$(MAKE) -C . run-jsc-$*

test-debugger: build-debugger-test-app build-dbg-testsuite
	dotnet test DebuggerTestSuite

run-browser-tests: $(BROWSER_TEST)/.stamp-browser-test-suite
	(cd $(BROWSER_TEST) && npm test)

run-browser-threads-tests: $(BROWSER_TEST_THREADS)/.stamp-browser-test-threads-suite
	(cd $(BROWSER_TEST_THREADS) && npm test)

clean: clean-browser-tests
	$(RM) -r *.o
	$(RM) -r ./obj
	$(RM) -r ./bin
	$(RM) -r .stamp-build-debug-sample
	$(RM) -r .stamp-build-test-suite
	$(RM) -r sample.dll
	$(RM) -r Simple.Dependency.dll
	$(RM) -r packager.exe
	$(RM) -r mini_tests.dll
	$(RM) -r main.exe
	$(RM) -r binding-tests.dll
	$(RM) -r $(WASM_FRAMEWORK)/.stamp-framework
	$(RM) -r $(WASM_FRAMEWORK)/netstandard2.0
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.dll
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.pdb
	$(RM) $(WASM_FRAMEWORK)/WebAssembly.*.xml
	$(RM) -r builds
	$(RM) emsdk_env.sh
	$(RM) .configured

package: build build-dbg-proxy
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
	cp framework/WebAssembly.Net.Http.dll tmp/framework
	cp framework/WebAssembly.Net.Http.pdb tmp/framework
	cp framework/WebAssembly.Net.Http.xml tmp/framework
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
	cp Mono.WebAssembly.DebuggerProxy/bin/Debug/netstandard2.0/Mono.WebAssembly.DebuggerProxy.dll tmp/
	cp Mono.WebAssembly.DebuggerProxy/bin/Debug/netstandard2.0/Mono.WebAssembly.DebuggerProxy.pdb tmp/
	mkdir tmp/dbg-proxy
	cp -r ProxyDriver/bin/Debug/netcoreapp2.1/ tmp/dbg-proxy/
	mkdir tmp/docs
	cp -r ./docs/ tmp/docs/
	mkdir tmp/packages
	cp -r ./sdk/packages tmp/
	mkdir tmp/Templates
	cp -r ./sdk/Templates tmp/
	mkdir tmp/support
	cp -r $(ZLIB_HEADERS) tmp/support
	(cd tmp;  zip -r9 ../mono-wasm-$(shell git rev-parse --short HEAD).zip .)
	rm -rf tmp

serve:
	python server.py

canary:
	/Applications/Google\ Chrome\ Canary.app/Contents/MacOS/Google\ Chrome\ Canary --remote-debugging-port=9222

check-aot: do-aot-sample

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
	$(RM) -r $(BROWSER_TEST)/source/**/bin $(BROWSER_TEST)/source/**/**/bin $(BROWSER_TEST)/source/**/obj $(BROWSER_TEST)/source/**/**/obj publish
