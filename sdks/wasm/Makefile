TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

MAKEFLAGS += --no-builtin-rules

#Use either 'release' or 'debug' dependending on what you need
DRIVER_CONF=debug


EMCC=source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && emcc
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

.stamp-jsvu:
	npm install jsvu -g
	jsvu --engines=all
	touch $@

.PHONY: toolchain
toolchain: .stamp-jsvu

driver.o: driver.c
	$(EMCC) -g -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 driver.c -c -o driver.o

debug/:
	mkdir -p $@
release/:
	mkdir -p $@

debug/.stamp-build: driver.o library_mono.js $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a | debug/
	$(EMCC) -g4 -Os -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 -s ASSERTIONS=2 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o debug/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
	touch $@

# Notice that release/.stamp-build depends on debug/.stamp-build. This is the case as emcc is believed to not work well with parallel builds.
release/.stamp-build: driver.o library_mono.js $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a debug/.stamp-build | release/
	$(EMCC) -Oz --llvm-opts 2 --llvm-lto 1 -s WASM=1 -s ALLOW_MEMORY_GROWTH=1 -s BINARYEN=1 -s "BINARYEN_TRAP_MODE='clamp'" -s TOTAL_MEMORY=134217728 -s ALIASING_FUNCTION_POINTERS=0 --js-library library_mono.js driver.o $(TOP)/sdks/out/wasm-interp/lib/libmonosgen-2.0.a -o release/mono.js -s NO_EXIT_RUNTIME=1 -s "EXTRA_EXPORTED_RUNTIME_METHODS=['ccall', 'FS_createPath', 'FS_createDataFile', 'cwrap', 'setValue', 'getValue', 'UTF8ToString']"
	touch $@


mono.js: $(DRIVER_CONF)/.stamp-build
	cp $(DRIVER_CONF)/mono.js $@

mono.wasm: $(DRIVER_CONF)/.stamp-build
	cp $(DRIVER_CONF)/mono.wasm $@

build-native: mono.js mono.wasm debug/.stamp-build release/.stamp-build

do-runtime:
	make -j4 -C ../builds build-wasm-interp package-wasm-interp
	make build-native

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
	csc /debug:portable /noconfig /nostdlib /unsafe -out:$@ $(TEST_MCS_DEPS) $(APP_SOURCES)

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
	csc /debug:portable /nostdlib /noconfig /unsafe -target:library -out:$@ -define:__MOBILE__,ARCH_32 $(TEST_MCS_DEPS) $(MINI_TEST_SOURCES) mini-test-runner.cs 

build-managed: managed/nunitlite.dll managed/mini_tests.dll managed/main.exe managed/wasm_corlib_test.dll managed/wasm_System_test.dll managed/wasm_System.Core_test.dll

build: build-native build-managed

CHACKA=~/.jsvu/ch
D8=~/.jsvu/v8
JSC=~/.jsvu/jsc
SM=~/.jsvu/sm

run-ch-%: toolchain build
	$(CHACKA) test.js -args $*

run-v8-%: toolchain build
	$(D8) --expose_wasm test.js -- $*

run-jsc-%: toolchain build
	$(JSC) test.js -- $*

run-sm-%: toolchain build
	$(SM) test.js $*

# Leaving JSC for now cuz it aborts when it encounters wasm
run-all-%:
	make -C . run-ch-$*
	make -C . run-v8-$*
	make -C . run-sm-$*

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
