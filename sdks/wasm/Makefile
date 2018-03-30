TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

MAKEFLAGS += --no-builtin-rules

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

APP_SOURCES = \
	main.cs	\
	incrementalrunner.cs

MINI_TEST_SOURCES = $(patsubst %,$(MINI_PATH)/%,$(MINI_TEST_FILES))

TEST_DEPS = $(patsubst %,managed/%,$(BCL_ASSEMBLIES)) managed/nunitlite.dll
TEST_MCS_DEPS = $(patsubst %,-r:%, $(TEST_DEPS))


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
	$(EMCC) -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=2 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o mono.js

build-native: mono.js

managed/:
	mkdir -p $@

$(patsubst %,managed/%,$(BCL_ASSEMBLIES)): managed/%: $(WASM_BCL_DIR)/% | managed/
	cp $< $@

#FIXME change BCL sdk build to include nunitlite.dll in out/bcl/wasm/
managed/nunitlite.dll: $(TOP)/mcs/class/lib/wasm/nunitlite.dll | managed/
	cd $(TOP)/mcs/tools/nunit-lite && make PROFILE=wasm
	cp $< $@

managed/main.exe: $(TEST_DEPS) $(APP_SOURCES) | managed/
	mcs /nostdlib /unsafe -out:$@ $(TEST_MCS_DEPS) $(APP_SOURCES)

managed/mini_tests.dll: $(TEST_DEPS) $(MINI_TEST_SOURCES) mini-test-runner.cs | managed/
	mcs /nostdlib /unsafe -target:library -out:$@ -define:__MOBILE__,ARCH_32 $(TEST_MCS_DEPS) $(MINI_TEST_SOURCES) mini-test-runner.cs 

build-managed: managed/nunitlite.dll managed/mini_tests.dll managed/main.exe

build: build-native build-managed

run: toolchain build
	$(D8) --expose_wasm test.js

clean:
	
package:
