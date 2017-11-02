TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/paths.mk

EMCC=source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && emcc
D8=$(TOP)/sdks/wasm/v8/out.gn/x64.release/d8
WASM_BCL_DIR=$(TOP)/sdks/out/bcl/wasm
MANAGED_DEPLOY_DIR=$(TOP)/sdks/wasm/managed
MINI_PATH=$(TOP)/mono/mini

BCL_ASSEMBLIES= \
        mscorlib.dll    \
        System.dll      \
        System.Core.dll

DEPS_ASSEMBLIES= \
        nunitlite.dll

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


BCL_INPUT_ASM = $(patsubst %,$(WASM_BCL_DIR)/%,$(BCL_ASSEMBLIES))
BCL_OUTPUT_ASM = $(patsubst %,$(MANAGED_DEPLOY_DIR)/%,$(BCL_ASSEMBLIES))
DEPS_FILES = $(patsubst %,$(MANAGED_DEPLOY_DIR)/%,$(DEPS_ASSEMBLIES))

MINI_TEST_SOURCES= $(patsubst %,$(MINI_PATH)/%,$(MINI_TEST_FILES))
MINI_TEST_DEPS= $(patsubst %,-r:%,$(BCL_OUTPUT_ASM) $(DEPS_FILES))

.stamp-depot-tools:
	git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git depot_tools
	touch $@

.stamp-v8: .stamp-depot-tools
	PATH=$(TOP)/sdks/wasm/depot_tools:$$PATH fetch v8
	cd v8 && tools/dev/v8gen.py x64.release
	cd v8 && $(TOP)/sdks/wasm/depot_tools/ninja -C out.gn/x64.release
	touch $@

.PHONY: toolchain
toolchain: .stamp-v8

driver.o: driver.c
	$(EMCC) -g -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 driver.c -c -o driver.o

mono.js: driver.o library_mono.js $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a
	$(EMCC) -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o mono.js

build-native: mono.js

#FIXME some Make magic here would be appreciated
.stamp-copy-bcl: $(TOP)/sdks/wasm/managed $(BCL_INPUT_ASM)
	mkdir -p managed/
	cp $(BCL_INPUT_ASM) managed/
	touch $@

managed/main.exe: main.cs .stamp-copy-bcl managed/nunitlite.dll
	mcs /nostdlib /unsafe main.cs -r:managed/nunitlite.dll $(MINI_TEST_DEPS) -out:managed/main.exe

managed/mini_tests.dll: $(MINI_TEST_SOURCES) mini-test-runner.cs .stamp-copy-bcl managed/nunitlite.dll
	mcs /nostdlib /unsafe -target:library -out:managed/mini_tests.dll -define:__MOBILE__,ARCH_32 $(MINI_TEST_DEPS) $(MINI_TEST_SOURCES) mini-test-runner.cs 

managed/nunitlite.dll: $(TOP)/mcs/class/lib/wasm/nunitlite.dll
	cd $(TOP)/mcs/tools/nunit-lite && make PROFILE=wasm
	cp  $< $@

build-managed: .stamp-copy-bcl managed/nunitlite.dll managed/mini_tests.dll managed/main.exe

build: build-native build-managed


run: toolchain build
	$(D8) --expose_wasm test.js

clean:
	
package:
