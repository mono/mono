TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/versions.mk
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

APP_SOURCES = \
	main.cs	\
	incrementalrunner.cs

MINI_TEST_SOURCES = $(patsubst %,$(MINI_PATH)/%,$(MINI_TEST_FILES))

TEST_DEPS = $(patsubst %,managed/%,$(BCL_ASSEMBLIES)) managed/nunitlite.dll
TEST_MCS_DEPS = $(patsubst %,-r:%, $(TEST_DEPS))


.stamp-depot-tools:
	rm -rf $(TOP)/sdks/wasm/depot_tools
	git clone https://chromium.googlesource.com/chromium/tools/depot_tools.git $(TOP)/sdks/wasm/depot_tools
	touch $@

.stamp-v8: .stamp-depot-tools
	PATH=$(TOP)/sdks/wasm/depot_tools:$$PATH fetch v8
	cd v8 && tools/dev/v8gen.py -vv x64.release
	cd v8 && $(TOP)/sdks/wasm/depot_tools/ninja -C out.gn/x64.release
	touch $@

.PHONY: toolchain
toolchain: .stamp-v8

driver.o: driver.c
	$(EMCC) -g -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 driver.c -c -o driver.o

debug/:
	mkdir -p $@
release/:
	mkdir -p $@

debug/mono.js: driver.o library_mono.js $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a | debug/
	$(EMCC) -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=2 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o debug/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"

release/mono.js: driver.o library_mono.js $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a | release/
	$(EMCC) -Oz --llvm-opts 2 --llvm-lto 1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o release/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"

mono.js: release/mono.js
	cp release/mono.js mono.js

mono.wasm: release/mono.wasm
	cp $< $@

build-native: mono.js mono.wasm debug/mono.js release/mono.js

managed/:
	mkdir -p $@

$(patsubst %,managed/%,$(BCL_ASSEMBLIES)): managed/%: $(WASM_BCL_DIR)/% | managed/
	cp $< $@

$(TOP)/mcs/class/lib/wasm/nunitlite.dll: FORCE
	$(MAKE) -C $(TOP)/mcs/tools/nunit-lite PROFILE=wasm

#FIXME change BCL sdk build to include nunitlite.dll in out/bcl/wasm/
managed/nunitlite.dll: $(TOP)/mcs/class/lib/wasm/nunitlite.dll | managed/
	cp $< $@

managed/main.exe: $(TEST_DEPS) $(APP_SOURCES) | managed/
	mcs /nostdlib /unsafe -out:$@ $(TEST_MCS_DEPS) $(APP_SOURCES)

$(TOP)/mcs/class/lib/wasm/tests/wasm_corlib_test.dll: FORCE
	$(MAKE) -C "$(TOP)/mcs/class/corlib" test-local PROFILE=wasm

managed/wasm_corlib_test.dll: $(TOP)/mcs/class/lib/wasm/tests/wasm_corlib_test.dll
	cp $< $@

$(TOP)/mcs/class/lib/wasm/tests/wasm_System_test.dll: FORCE
	$(MAKE) -C "$(TOP)/mcs/class/System" test-local PROFILE=wasm

managed/wasm_System_test.dll: $(TOP)/mcs/class/lib/wasm/tests/wasm_System_test.dll
	cp $< $@

$(TOP)/mcs/class/lib/wasm/tests/wasm_System.Core_test.dll: FORCE
	$(MAKE) -C "$(TOP)/mcs/class/System.Core" test-local PROFILE=wasm

managed/wasm_System.Core_test.dll: $(TOP)/mcs/class/lib/wasm/tests/wasm_System.Core_test.dll
	cp $< $@

managed/mini_tests.dll: $(TEST_DEPS) $(MINI_TEST_SOURCES) mini-test-runner.cs | managed/
	mcs /nostdlib /unsafe -target:library -out:$@ -define:__MOBILE__,ARCH_32 $(TEST_MCS_DEPS) $(MINI_TEST_SOURCES) mini-test-runner.cs 

build-managed: managed/nunitlite.dll managed/mini_tests.dll managed/main.exe managed/wasm_corlib_test.dll managed/wasm_System_test.dll managed/wasm_System.Core_test.dll

build: build-native build-managed

run-%: toolchain build
	$(D8) --expose_wasm test.js -- $*

clean:
	
package: build
	rm -rf tmp
	mkdir tmp
	mkdir tmp/bcl
	cp -r $(TOP)/sdks/out/bcl/wasm/* tmp/bcl/
	cp driver.c tmp/
	cp $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a tmp/
	cp library_mono.js tmp/
	cp mono.js tmp/
	cp -r debug tmp/
	cp -r release tmp/
	cp sample.html tmp/
	cp sample.cs tmp/
	cp README.md tmp/
	cp server.py tmp/
	(cd tmp;  zip -r9 ../mono-wasm-$(shell git rev-parse --short HEAD).zip .)
	rm -rf tmp

FORCE:
