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


EMCC=source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && emcc
WASM_BCL_DIR=$(TOP)/sdks/out/bcl/wasm
MINI_PATH=$(TOP)/mono/mini
CSC= MONO_PATH=$(TOP)/mcs/class/lib/build $(TOP)/sdks/builds/bcl/runtime/mono-wrapper $(CSC_LOCATION)
CSC_FLAGS := /debug:portable /noconfig /nostdlib /nologo /langversion:latest

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
	bindings.cs	\
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
	$(EMCC) -g -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 driver.c -c -o driver.o

debug/:
	mkdir -p $@
release/:
	mkdir -p $@

debug/.stamp-build: driver.o library_mono.js binding_support.js $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a | debug/
	$(EMCC) -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=2 --js-library library_mono.js --js-library binding_support.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o debug/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
	touch $@

# Notice that release/.stamp-build depends on debug/.stamp-build. This is the case as emcc is believed to not work well with parallel builds.
release/.stamp-build: driver.o library_mono.js binding_support.js $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a debug/.stamp-build | release/
	$(EMCC) -Oz --llvm-opts 2 --llvm-lto 1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 --js-library library_mono.js --js-library binding_support.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o release/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
	touch $@

mono.js: $(DRIVER_CONF)/.stamp-build
	cp $(DRIVER_CONF)/mono.js $@

mono.wasm: $(DRIVER_CONF)/.stamp-build
	cp $(DRIVER_CONF)/mono.wasm $@

build-native: mono.js mono.wasm debug/.stamp-build release/.stamp-build

do-runtime:
	make -j4 -C ../builds package-wasm-interp
	make -j4 -C ../builds package-bcl
	make build-native

BCL_DEPS=/r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.Core.dll /r:$(WASM_BCL_DIR)/System.dll

main.exe: $(APP_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -out:$@ $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(APP_SOURCES)

mini_tests.dll: $(MINI_TEST_SOURCES) mini-test-runner.cs
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ -define:__MOBILE__,ARCH_32 $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll  $(MINI_TEST_SOURCES) mini-test-runner.cs

binding_tests.dll: $(BINDING_TEST_SOURCES)
	$(CSC) $(CSC_FLAGS) /unsafe -target:library -out:$@ $(BCL_DEPS) /r:$(WASM_BCL_DIR)/nunitlite.dll $(BINDING_TEST_SOURCES)

bindings.dll: bindings.cs
	$(CSC) /debug:portable /noconfig /nostdlib /target:library -out:$@ /r:$(WASM_BCL_DIR)/mscorlib.dll bindings.cs

sample.dll:  $(TEST_DEPS) sample.cs
	$(CSC) /debug:portable /noconfig /nostdlib /target:library -out:$@ /r:$(WASM_BCL_DIR)/mscorlib.dll /r:$(WASM_BCL_DIR)/System.Core.dll sample.cs

Mono.Cecil.dll: $(TOP)/sdks/out/bcl/net_4_x/Mono.Cecil.dll
	cp $< $@

# We use system csc as this is a desktop program
packager.exe: packager.cs Mono.Cecil.dll
	csc /debug:portable -r:Mono.Cecil.dll packager.cs

.stamp-build-debug-sample: $(DRIVER_CONF)/.stamp-build packager.exe bindings.dll sample.dll debug.html runtime.g.js
	mono packager.exe -debug -out=debug_sample sample.dll
	cp debug.html debug_sample
	touch $@

TEST_ASSEMBLIES = $(WASM_BCL_DIR)/nunitlite.dll $(WASM_BCL_DIR)/tests/wasm_corlib_test.dll $(WASM_BCL_DIR)/tests/wasm_System_test.dll $(WASM_BCL_DIR)/tests/wasm_System.Core_test.dll

.stamp-build-test-suite: $(DRIVER_CONF)/.stamp-build packager.exe bindings.dll binding_tests.dll mini_tests.dll main.exe runtime.g.js
	mono packager.exe -out=test-suite binding_tests.dll mini_tests.dll main.exe $(TEST_ASSEMBLIES)
	cp test.js test-suite/
	touch $@

build-debug-sample: .stamp-build-debug-sample

build-test-suite: .stamp-build-test-suite

build-managed: build-debug-sample build-test-suite

build: build-native build-managed

CHAKRA=~/.jsvu/ch
D8=~/.jsvu/v8
JSC=~/.jsvu/jsc
SM=~/.jsvu/sm

run-ch-%: toolchain build-test-suite
	(cd test-suite && $(CHAKRA) test.js -args $*)

run-v8-%: toolchain build-test-suite
	(cd test-suite && $(D8) --expose_wasm test.js -- $*)

run-jsc-%: toolchain build-test-suite
	(cd test-suite && $(JSC) test.js -- $*)

run-sm-%: toolchain build-test-suite
	(cd test-suite && $(SM) test.js $*)

# Leaving JSC for now cuz it aborts when it encounters wasm
run-all-%:
	make -C . run-ch-$*
	make -C . run-v8-$*
	make -C . run-sm-$*
	make -C . run-jsc-$*

clean:
	
package: build
	rm -rf tmp
	mkdir tmp
	mkdir tmp/bcl
	cp -r $(TOP)/sdks/out/bcl/wasm/* tmp/bcl/
	rm -rf tmp/bcl/bare
	rm -rf tmp/bcl/secxml
	rm -rf tmp/bcl/tests
	rm -rf tmp/bcl/corlib.unsafe.dll.tmp
	cp driver.c tmp/
	cp $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a tmp/
	cp library_mono.js tmp/
	cp binding_support.js tmp/
	cp -r debug tmp/
	cp -r release tmp/
	rm tmp/debug/.stamp-build
	rm tmp/release/.stamp-build
	cp sample.html tmp/
	mkdir tmp/framework
	cp bindings.cs tmp
	cp bindings.dll tmp/framework
	cp bindings.pdb tmp/framework
	cp sample.cs tmp/
	cp README.md tmp/
	cp server.py tmp/
	cp Mono.Cecil.dll tmp/
	cp packager.exe tmp/
	cp runtime.g.js tmp/
	(cd tmp;  zip -r9 ../mono-wasm-$(shell git rev-parse --short HEAD).zip .)
	rm -rf tmp

serve:
	python server.py

canary:
	/Applications/Google\ Chrome\ Canary.app/Contents/MacOS/Google\ Chrome\ Canary --remote-debugging-port=9222
