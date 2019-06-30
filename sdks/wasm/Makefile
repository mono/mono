TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
-include $(TOP)/mcs/build/config.make
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

all: build

MAKEFLAGS += --no-builtin-rules
#emcc has lots of bash'isms
SHELL:=/bin/bash

#Use either 'release' or 'debug' dependending on what you need
DRIVER_CONF=release

CSC_LOCATION?=$(STANDALONE_CSC_LOCATION)

EMSCRIPTEN_SDK_DIR?=$(TOP)/sdks/builds/toolchains/emsdk
EMSCRIPTEN_DIR?=$(EMSCRIPTEN_SDK_DIR)/upstream/emscripten
EMCC=$(EMSCRIPTEN_DIR)/emcc
EMCC_DYNAMIC=EMCC_FORCE_STDLIBS=1 $(EMCC)
WASM_BCL_DIR=$(TOP)/sdks/out/wasm-bcl/wasm
WASM_RUNTIME_DIR=$(TOP)/sdks/out/wasm-runtime-release
MINI_PATH=$(TOP)/mono/mini
CSC?= MONO_PATH=$(TOP)/mcs/class/lib/build $(TOP)/sdks/builds/bcl/runtime/mono-wrapper $(CSC_LOCATION)
CSC_FLAGS := /debug:portable /noconfig /nostdlib /nologo /langversion:latest
API_REFS=$(TOP)/external/binary-reference-assemblies/v4.6
MONO_SUPPORT=$(WASM_RUNTIME_DIR)/include/support

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

driver-dynamic.o: driver.c corebindings.c pinvoke-tables-default.h
	$(EMCC) -g -Os -fPIC -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s ALIASING_FUNCTION_POINTERS=0 -DWASM_SUPPORTS_DLOPEN -DCORE_BINDINGS -I$(WASM_RUNTIME_DIR)/include/mono-2.0 driver.c -c -o driver-dynamic.o

corebindings-dynamic.o: corebindings.c
	$(EMCC) -g -Os -fPIC -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s ALIASING_FUNCTION_POINTERS=0 -I$(WASM_RUNTIME_DIR)/include/mono-2.0 corebindings.c -c -o $@

MONO_LIBS = $(TOP)/sdks/out/wasm-runtime-release/lib/{libmono-ee-interp.a,libmono-native.a,libmono-icall-table.a,libmonosgen-2.0.a,libmono-ilgen.a}

EMCC_FLAGS=-s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s ALIASING_FUNCTION_POINTERS=0 -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString', 'addFunction']" -s USE_ZLIB=1 -s "EXPORTED_FUNCTIONS=['_putchar']" --source-map-base http://example.com
EMCC_DEBUG_FLAGS =-g4 -Os -s -s ASSERTIONS=1
EMCC_RELEASE_FLAGS=-Oz --llvm-opts 2 --llvm-lto 1
EMCC_THREADS_FLAGS=-s ALLOW_MEMORY_GROWTH=0 -s USE_PTHREADS=1 -s TOTAL_MEMORY=536870912 -pthread -Wl,--shared-memory,--no-check-features -s PTHREAD_POOL_SIZE=2

#
# Interpreter builds
#

# $(1) - name
# $(2) - runtime dir
# $(3) - EMCC_FLAGS
define InterpBuildTemplate

$(1)/mono.js: $(1)/driver.o $(1)/corebindings.o $(1)/zlib-helper.o library_mono.js binding_support.js dotnet_support.js $(TOP)/sdks/out/$(2)/lib/libmonosgen-2.0.a | $(1)/
	$(EMCC) $(EMCC_FLAGS) $(3) --js-library library_mono.js --js-library binding_support.js --js-library dotnet_support.js $(1)/driver.o $(1)/corebindings.o $(1)/zlib-helper.o $(TOP)/sdks/out/${2}/lib/{libmono-ee-interp.a,libmono-native.a,libmono-icall-table.a,libmonosgen-2.0.a,libmono-ilgen.a} -o $(1)/mono.js

$(1)/driver.o: driver.c corebindings.c pinvoke-tables-default.h | $(1)/
	$(EMCC) $(EMCC_FLAGS) -DCORE_BINDINGS -I$(WASM_RUNTIME_DIR)/include/mono-2.0 driver.c -c -o $$@

$(1)/corebindings.o: corebindings.c | $(1)/
	$(EMCC) $(EMCC_FLAGS) -I$(WASM_RUNTIME_DIR)/include/mono-2.0 corebindings.c -c -o $$@

$(1)/zlib-helper.o: zlib-helper.c | $(1)/
	$(EMCC) $(EMCC_FLAGS) -I$(WASM_RUNTIME_DIR)/include/mono-2.0 -I$(WASM_RUNTIME_DIR)/include/support zlib-helper.c -c -o $$@

build-native: $(1)/mono.js

$(1)/:
	mkdir -p $$@

endef

$(eval $(call InterpBuildTemplate,debug,wasm-runtime-release,$(EMCC_DEBUG_FLAGS)))
$(eval $(call InterpBuildTemplate,release,wasm-runtime-release,$(EMCC_RELEASE_FLAGS)))
ifdef ENABLE_WASM_THREADS
$(eval $(call InterpBuildTemplate,threads-debug,wasm-runtime-threads-release,$(EMCC_DEBUG_FLAGS) $(EMCC_THREADS_FLAGS)))
$(eval $(call InterpBuildTemplate,threads-release,wasm-runtime-threads-release,$(EMCC_RELEASE_FLAGS) $(EMCC_THREADS_FLAGS)))
endif

#release-dynamic/.stamp-build: driver-dynamic.o corebindings-dynamic.o library_mono.js binding_support.js dotnet_support.js $(TOP)/sdks/out/wasm-runtime-release/lib/libmonosgen-2.0.a release/.stamp-build | release-dynamic/
#	$(EMCC_DYNAMIC) $(EMCC_FLAGS) -s MAIN_MODULE=1 -s EXPORT_ALL=1 -s DISABLE_EXCEPTION_CATCHING=0 -s RESERVED_FUNCTION_POINTERS=64 -Oz --llvm-opts 2 --llvm-lto 1 --js-library library_mono.js --js-library binding_support.js --js-library dotnet_support.js driver-dynamic.o corebindings-dynamic.o $(MONO_LIBS) -o release-dynamic/mono.js -s "EXPORTED_FUNCTIONS=['_putchar']"
#	touch $@

runtime:
	$(MAKE) -C ../builds package-wasm-runtime

cross:
	$(MAKE) -C ../builds package-wasm-cross

bcl:
	$(MAKE) -C ../builds package-wasm-bcl

tuner:
	$(MAKE) -C ../../mcs/tools/wasm-tuner PROFILE=wasm_tools
	cp ../../mcs/class/lib/wasm_tools/wasm-tuner.exe* ../out/wasm-bcl/wasm_tools/wasm-tuner.exe*

cil-strip:
	$(MAKE) -C ../../mcs/tools/cil-strip  PROFILE=wasm_tools

mono: runtime cross bcl

BCL_DEPS=/r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.Core.dll /r:$(WASM_BCL_DIR)/System.dll /r:$(WASM_BCL_DIR)/System.Net.Http.dll /r:$(WASM_BCL_DIR)/Facades/netstandard.dll /r:$(WASM_BCL_DIR)/System.IO.Compression.dll /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll 

main.exe: $(APP_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -out:$@ $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(APP_SOURCES)

mini_tests.dll: $(MINI_TEST_SOURCES) mini-test-runner.cs
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ -define:__MOBILE__,ARCH_32 $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(MINI_TEST_SOURCES) mini-test-runner.cs

binding_tests.dll: $(WASM_FRAMEWORK)/.stamp-framework $(BINDING_TEST_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ $(WASM_FRAMEWORK_DEPS) $(BCL_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll /r:$(WASM_BCL_DIR)/nunitlite.dll $(BINDING_TEST_SOURCES)

$(WASM_FRAMEWORK)/.stamp-framework: $(WASM_FRAMEWORK_SOURCES)
	dotnet build $(WASM_FRAMEWORK_SOURCE)/WebAssembly.Framework.sln
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
packager.exe: packager.cs Mono.Cecil.dll $(OPTIONS_CS) build-native
	$(CSC) $(CSC_FLAGS) /out:$@ /r:Mono.Cecil.dll packager.cs $(OPTIONS_CS) /r:$(API_REFS)/mscorlib.dll /r:$(API_REFS)/System.dll /r:$(API_REFS)/System.Core.dll

.stamp-build-debug-sample: packager.exe $(WASM_FRAMEWORK)/.stamp-framework sample.dll debug.html runtime.js
	mono --debug packager.exe --copy=always -debug -out=debug_sample --asset=debug.html sample.dll
	touch $@	

TEST_ASSEMBLIES = $(WASM_BCL_DIR)/nunitlite.dll $(WASM_BCL_DIR)/tests/wasm_corlib_test.dll $(WASM_BCL_DIR)/tests/wasm_System_test.dll $(WASM_BCL_DIR)/tests/wasm_System.Core_test.dll

bin/test-suite/mono.js: packager.exe $(WASM_FRAMEWORK)/.stamp-framework binding_tests.dll mini_tests.dll main.exe runtime-tests.js
	mono packager.exe --template=runtime-tests.js --appdir=bin/test-suite --builddir=obj/test-suite binding_tests.dll mini_tests.dll main.exe $(TEST_ASSEMBLIES) --linker
	ninja -v -C obj/test-suite

build-test-suite: bin/test-suite/mono.js

$(BROWSER_TEST)/.stamp-browser-test-suite: packager.exe $(WASM_FRAMEWORK)/.stamp-framework $(BROWSER_TEST_SOURCES)
	$(CSC) $(CSC_FLAGS) /target:library -out:$(BROWSER_TEST)/HttpTestSuite.dll $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) $(BROWSER_TEST)/HttpTestSuite.cs 
	$(CSC) $(CSC_FLAGS) /target:library -out:$(BROWSER_TEST)/WebSocketTestSuite.dll $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) $(BROWSER_TEST)/WebSocketTestSuite.cs 
	$(CSC) $(CSC_FLAGS) /target:library -out:$(BROWSER_TEST)/BindingsTestSuite.dll $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll $(BROWSER_TEST)/BindingsTestSuite.cs 
	$(CSC) $(CSC_FLAGS) /target:library -out:$(BROWSER_TEST)/IssuesTestSuite.dll $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll $(BROWSER_TEST)/IssuesTestSuite.cs 
	$(CSC) $(CSC_FLAGS) /target:library -out:$(BROWSER_TEST)/ZipTestSuite.dll $(BCL_DEPS) $(WASM_FRAMEWORK_DEPS) /r:$(WASM_BCL_DIR)/Facades/System.Memory.dll $(BROWSER_TEST)/ZipTestSuite.cs 
	mono packager.exe -debug -out=$(BROWSER_TEST)/publish --copy=IfNewer --template=$(BROWSER_TEST)/runtime.js $(BROWSER_TEST_ASSETS) $(BROWSER_TEST)/HttpTestSuite.dll $(BROWSER_TEST)/WebSocketTestSuite.dll $(BROWSER_TEST)/BindingsTestSuite.dll $(BROWSER_TEST)/IssuesTestSuite.dll $(BROWSER_TEST)/ZipTestSuite.dll
	(cd $(BROWSER_TEST) && npm install)
	touch $@

.stamp-build-debugger-test-app: packager.exe binding_tests.dll debugger-test.dll debugger-driver.html
	mono packager.exe --copy=always -debugrt -debug --template=runtime.js --builddir=obj/debugger-test-suite --appdir=bin/debugger-test-suite --asset=debugger-driver.html debugger-test.dll
	ninja -v -C obj/debugger-test-suite
	touch $@

build-aot-sample: packager.exe hello.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDKDIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-sample --nobinding --builddir=obj/aot-sample --aot --template=runtime-tests.js --pinvoke-libs=libfoo hello.exe
	ninja -v -C obj/aot-sample

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

build-interp-sample: packager.exe hello.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/interp-sample --nobinding --builddir=obj/interp-sample --template=runtime-tests.js hello.exe --linker --link-icalls
	ninja -v -C obj/interp-sample

run-interp-sample: build-interp-sample
	(cd bin/interp-sample && $(SM) runtime.js --run hello.exe)

threads.exe: threads.cs
	$(CSC) $(CSC_FLAGS) -out:$@ $(BCL_DEPS) threads.cs

build-threads-sample: packager.exe threads.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDKDIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/threads-sample --nobinding --builddir=obj/threads-sample --template=runtime-tests.js --threads threads.exe
	ninja -v -C obj/threads-sample

run-threads-sample: build-threads-sample
	(cd bin/threads-sample && $(D8) --experimental-wasm-threads --stack-trace-limit=1000 runtime.js -- --run threads.exe)

build-aot-mini: packager.exe mini_tests.dll main.exe runtime.js
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDKDIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-mini --nobinding --builddir=obj/aot-mini --aot --template=runtime-tests.js mini_tests.dll
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

build-aot-profile: packager.exe aot-profile/main.exe
	mono --debug packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDK_DIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-profile --builddir=obj/aot-profile --profile=aot --template=aot-profile/runtime.js --asset=aot-profile/index.html aot-profile/main.exe
	ninja -C obj/aot-profile

data.aotprofile: build-aot-profile
	node aot-profile/gen-profile.js $@ bin/aot-profile

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
$(eval $(call AotTestTemplate,System.Core,wasm_System.Core_test.dll,system-core))
$(eval $(call AotTestTemplate,System,wasm_System_test.dll,system))

$(eval $(call AotTestTemplate,mixed-corlib,wasm_corlib_test.dll,corlib,--aot-interp --aot-assemblies=mscorlib))
$(eval $(call AotTestTemplate,mixed-System.Core,wasm_System.Core_test.dll,system-core,--aot-interp --aot-assemblies=mscorlib))

AOT_TEST_SUITES=corlib System.Core

build-aot-all: build
	$(MAKE) $(patsubst %,build-aot-%,$(AOT_TEST_SUITES))

run-aot-all: build-aot-all
	for suite in System.Core; do $(MAKE) run-aot-$$suite || exit 1; done

build-debug-sample: .stamp-build-debug-sample

build-debugger-test-app: .stamp-build-debugger-test-app
build-managed: build-debug-sample build-test-suite

build-dbg-proxy:
	dotnet build ProxyDriver

build-dbg-testsuite:
	dotnet build DebuggerTestSuite

build: build-native build-managed build-debugger-test-app

gen-pinvoke-tables:
	mono ../../mcs/class/lib/wasm_tools/wasm-tuner.exe --gen-pinvoke-table System.Native $(WASM_BCL_DIR)/mscorlib.dll > pinvoke-tables-default.h

CHAKRA=~/.jsvu/ch
D8=~/.jsvu/v8
JSC=~/.jsvu/jsc
SM=~/.jsvu/sm

run-ch-%: toolchain build-test-suite
	(cd bin/test-suite && $(CHAKRA) runtime.js -args $*)

run-v8-%: toolchain build-test-suite
	(cd bin/test-suite && $(D8) --expose_wasm runtime.js -- $*)

run-jsc-%: toolchain build-test-suite
	(cd bin/test-suite && $(JSC) runtime.js -- $*)

run-sm-%: toolchain build-test-suite
	(cd bin/test-suite && $(SM) runtime.js $*)

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

clean: clean-sdk
	$(RM) -r *.o
	$(RM) -r ./obj
	$(RM) -r ./bin
	$(RM) -r .stamp-build-debug-sample
	$(RM) -r .stamp-build-test-suite
	$(RM) -r $(BROWSER_TEST)/.stamp-browser-test-suite
	$(RM) -r $(BROWSER_TEST)/node-modules
	$(RM) -r $(BROWSER_TEST)/publish
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
	$(RM) -r debug
	$(RM) -r release
	$(RM) -r release-dynamic

package: build build-dbg-proxy build-sdk
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
	cp driver.c pinvoke-tables-default.h tmp/
	cp corebindings.c tmp/
	cp zlib-helper.c tmp/
	cp zlib-helper.o tmp/
	cp pinvoke-tables-default.h tmp/
	cp $(MONO_LIBS) tmp/
	cp library_mono.js tmp/
	cp binding_support.js tmp/
	cp dotnet_support.js tmp/
	cp -r debug tmp/
	cp -r release tmp/
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

build-sdk: $(WASM_FRAMEWORK)/.stamp-framework
	dotnet build sdk/MonoWasmSdkCLI.sln
