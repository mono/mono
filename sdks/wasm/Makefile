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


EMSCRIPTEN_SDKDIR=$(TOP)/sdks/builds/toolchains/emsdk
EMCC=source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && emcc
WASM_BCL_DIR=$(TOP)/sdks/out/wasm-bcl/wasm
WASM_RUNTIME_DIR=$(TOP)/sdks/out/wasm-runtime-release
MINI_PATH=$(TOP)/mono/mini
CSC?= MONO_PATH=$(TOP)/mcs/class/lib/build $(TOP)/sdks/builds/bcl/runtime/mono-wrapper $(CSC_LOCATION)
CSC_FLAGS := /debug:portable /noconfig /nostdlib /nologo /langversion:latest
API_REFS=$(TOP)/external/binary-reference-assemblies/v4.6

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

driver.o: driver.c
	$(EMCC) -g -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s ALIASING_FUNCTION_POINTERS=0 -I$(WASM_RUNTIME_DIR)/include/mono-2.0 driver.c -c -o driver.o

debug/:
	mkdir -p $@
release/:
	mkdir -p $@

MONO_LIBS = $(TOP)/sdks/out/wasm-runtime-release/lib/libmono-ee-interp.a $(TOP)/sdks/out/wasm-runtime-release/lib/libmonosgen-2.0.a $(TOP)/sdks/out/wasm-runtime-release/lib/libmono-ilgen.a $(TOP)/sdks/out/wasm-runtime-release/lib/libmono-icall-table.a

debug/.stamp-build: driver.o library_mono.js binding_support.js dotnet_support.js $(TOP)/sdks/out/wasm-runtime-release/lib/libmonosgen-2.0.a | debug/
	$(EMCC) -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=1 --js-library library_mono.js --js-library binding_support.js --js-library dotnet_support.js driver.o $(MONO_LIBS) -o debug/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
	touch $@

# Notice that release/.stamp-build depends on debug/.stamp-build. This is the case as emcc is believed to not work well with parallel builds.
release/.stamp-build: driver.o library_mono.js binding_support.js dotnet_support.js $(TOP)/sdks/out/wasm-runtime-release/lib/libmonosgen-2.0.a debug/.stamp-build | release/
	$(EMCC) -Oz --llvm-opts 2 --llvm-lto 1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s ALIASING_FUNCTION_POINTERS=0 --js-library library_mono.js --js-library binding_support.js --js-library dotnet_support.js driver.o $(MONO_LIBS) -o release/mono.js -s NO_EXIT_RUNTIME=1 -s "EXPORTED_FUNCTIONS=['_putchar']" -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
	touch $@

build-native: debug/.stamp-build release/.stamp-build

do-runtime:
	make -j4 -C ../builds package-wasm-runtime
	make -j4 -C ../builds package-wasm-bcl
	make build-native

BCL_DEPS=/r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.Core.dll /r:$(WASM_BCL_DIR)/System.dll /r:$(WASM_BCL_DIR)/System.Net.Http.dll

main.exe: $(APP_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -out:$@ $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(APP_SOURCES)

mini_tests.dll: $(MINI_TEST_SOURCES) mini-test-runner.cs
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ -define:__MOBILE__,ARCH_32 $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(MINI_TEST_SOURCES) mini-test-runner.cs

binding_tests.dll: WebAssembly.Bindings.dll $(BINDING_TEST_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ /r:WebAssembly.Bindings.dll $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll $(BINDING_TEST_SOURCES)

WebAssembly.Bindings.dll: bindings.cs
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:$(WASM_BCL_DIR)/mscorlib.dll bindings.cs

WebAssembly.Net.Http.dll: WebAssembly.Bindings.dll WasmHttpMessageHandler.cs
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:WebAssembly.Bindings.dll /r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.dll /r:$(WASM_BCL_DIR)/System.Core.dll /r:$(WASM_BCL_DIR)/System.Net.dll /r:$(WASM_BCL_DIR)/System.Net.Http.dll WasmHttpMessageHandler.cs

Simple.Dependency.dll: dependency.cs
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.Core.dll dependency.cs

sample.dll: Simple.Dependency.dll sample.cs WebAssembly.Bindings.dll WebAssembly.Net.Http.dll
	$(CSC) $(CSC_FLAGS) /target:library -out:$@ /r:Simple.Dependency.dll /r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.dll /r:$(WASM_BCL_DIR)/System.Core.dll /r:$(WASM_BCL_DIR)/System.Net.Http.dll /r:WebAssembly.Bindings.dll /r:WebAssembly.Net.Http.dll sample.cs

OPTIONS_CS = $(TOP)/mcs/class/Mono.Options/Mono.Options/Options.cs

Mono.Cecil.dll: $(TOP)/mcs/class/lib/wasm_tools/Mono.Cecil.dll
	cp $< $@

# Should use .csproj file and msbuild
packager.exe: packager.cs Mono.Cecil.dll $(OPTIONS_CS)
	$(CSC) $(CSC_FLAGS) /out:$@ /r:Mono.Cecil.dll packager.cs $(OPTIONS_CS) /r:$(API_REFS)/mscorlib.dll /r:$(API_REFS)/System.dll /r:$(API_REFS)/System.Core.dll

.stamp-build-debug-sample: $(DRIVER_CONF)/.stamp-build packager.exe WebAssembly.Bindings.dll WebAssembly.Net.Http.dll sample.dll debug.html runtime.js
	mono packager.exe -debug -out=debug_sample sample.dll
	cp debug.html debug_sample
	touch $@	

TEST_ASSEMBLIES = $(WASM_BCL_DIR)/nunitlite.dll $(WASM_BCL_DIR)/tests/wasm_corlib_test.dll $(WASM_BCL_DIR)/tests/wasm_System_test.dll $(WASM_BCL_DIR)/tests/wasm_System.Core_test.dll

.stamp-build-test-suite: $(DRIVER_CONF)/.stamp-build packager.exe WebAssembly.Bindings.dll binding_tests.dll WebAssembly.Net.Http.dll mini_tests.dll main.exe runtime-tests.js
	mono --debug packager.exe --template=runtime-tests.js --appdir=bin/test-suite --builddir=obj/test-suite binding_tests.dll WebAssembly.Net.Http.dll mini_tests.dll main.exe $(TEST_ASSEMBLIES)
	ninja -v -C obj/test-suite
	touch $@

# For development
aot-sample: packager.exe mini_tests.dll main.exe runtime.js
	mono packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDKDIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-sample --nobinding --builddir=obj/aot-sample --aot --template=runtime-tests.js mini_tests.dll
	ninja -v -C obj/aot-sample

do-aot-sample: aot-sample
	(cd bin/aot-sample && $(SM) runtime.js --regression mini_tests.dll)

build-aot-mini: packager.exe mini_tests.dll main.exe runtime.js
	mono packager.exe --emscripten-sdkdir=$(EMSCRIPTEN_SDKDIR) --mono-sdkdir=$(TOP)/sdks/out -appdir=bin/aot-mini --nobinding --builddir=obj/aot-mini --aot --template=runtime-tests.js mini_tests.dll
	ninja -v -C obj/aot-mini

check-aot-mini run-aot-mini: build-aot-mini
	(cd bin/aot-mini && $(SM) runtime.js --regression mini_tests.dll)

##
# Parameters:
# $(1) - make target suffix
# $(2) - test assembly filename
# $(3) - main.exe argument
define AotTestTemplate

build-aot-$(1): packager.exe runtime-tests.js main.exe WebAssembly.Bindings.dll binding_tests.dll $$(WASM_BCL_DIR)/nunitlite.dll $$(WASM_BCL_DIR)/tests/$(2)
	mono packager.exe --emscripten-sdkdir=$$(EMSCRIPTEN_SDKDIR) --mono-sdkdir=$$(TOP)/sdks/out -appdir=bin/aot-$(1) --nobinding --builddir=obj/aot-$(1) --aot --template=runtime-tests.js main.exe WebAssembly.Bindings.dll binding_tests.dll $$(WASM_BCL_DIR)/tests/$(2)
	ninja -v -C obj/aot-$(1)

clean-aot-$(1):
	$(RM) -r obj/aot-$(1) bin/aot-$(1)

check-aot-$(1) run-aot-$(1): build-aot-$(1)
	(cd bin/aot-$(1) && $$(SM) runtime.js $(3))

endef

$(eval $(call AotTestTemplate,corlib,wasm_corlib_test.dll,corlib))
$(eval $(call AotTestTemplate,System.Core,wasm_System.Core_test.dll,system-core))
$(eval $(call AotTestTemplate,System,wasm_System_test.dll,system))

AOT_TEST_SUITES=corlib System.Core

build-aot-all:
	$(MAKE) $(patsubst %,build-aot-%,$(AOT_TEST_SUITES))

run-aot-all:
	for suite in System.Core; do make run-aot-$$suite || exit 1; done

build-debug-sample: .stamp-build-debug-sample

build-test-suite: .stamp-build-test-suite

build-managed: build-debug-sample build-test-suite

build-dbg-proxy:
	dotnet build ProxyDriver

build-framework-nuget:
	nuget pack WebAssembly.Framework.nuspec

build: build-native build-managed

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
	make -C . run-ch-$*
	make -C . run-v8-$*
	make -C . run-sm-$*
	make -C . run-jsc-$*

clean:

package: build build-dbg-proxy build-framework-nuget
	rm -rf tmp
	mkdir tmp
	mkdir tmp/wasm-bcl
	mkdir tmp/wasm-bcl/wasm
	cp -r $(TOP)/sdks/out/wasm-bcl/wasm/* tmp/wasm-bcl/wasm/
	rm -rf tmp/wasm-bcl/wasm/bare
	rm -rf tmp/wasm-bcl/wasm/secxml
	rm -rf tmp/wasm-bcl/wasm/tests
	rm -rf tmp/wasm-bcl/wasm/corlib.unsafe.dll.tmp
	cp driver.c tmp/
	cp $(MONO_LIBS) tmp/
	cp library_mono.js tmp/
	cp binding_support.js tmp/
	cp dotnet_support.js tmp/
	cp -r debug tmp/
	cp -r release tmp/
	rm tmp/debug/.stamp-build
	rm tmp/release/.stamp-build
	cp bindings.cs tmp
	mkdir tmp/framework
	cp WebAssembly.Bindings.dll tmp/framework
	cp WebAssembly.Bindings.pdb tmp/framework
	cp WasmHttpMessageHandler.cs tmp
	cp WebAssembly.Net.Http.dll tmp/framework
	cp WebAssembly.Net.Http.pdb tmp/framework
	cp WebAssembly.Framework.0.1.0-alpha.nupkg tmp/framework
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
	(cd tmp;  zip -r9 ../mono-wasm-$(shell git rev-parse --short HEAD).zip .)
	rm -rf tmp

serve:
	python server.py

canary:
	/Applications/Google\ Chrome\ Canary.app/Contents/MacOS/Google\ Chrome\ Canary --remote-debugging-port=9222

check-aot: do-aot-sample
