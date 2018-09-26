#emcc has lots of bash'isms
SHELL:=/bin/bash

WASM_RUNTIME_AC_VARS= \
	ac_cv_func_shm_open_working_with_mmap=no

WASM_RUNTIME_CONFIGURE_FLAGS = \
	$(if $(ENABLE_CXX),--enable-cxx) \
	--cache-file=$(TOP)/sdks/builds/wasm-runtime.config.cache \
	--prefix=$(TOP)/sdks/out/wasm-runtime \
	--disable-mcs-build \
	--disable-nls \
	--disable-boehm \
	--disable-btls \
	--with-lazy-gc-thread-creation=yes \
	--with-libgc=none \
	--disable-executables \
	--disable-support-build \
	--disable-visibility-hidden \
	--enable-maintainer-mode	\
	--enable-minimal=ssa,com,jit,reflection_emit_save,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_marksweep_conc,sgen_split_nursery,sgen_gc_bridge,logging,remoting,shared_perfcounters,sgen_debug_helpers,soft_debug,interpreter \
	--host=wasm32 \
	--enable-llvm-runtime \
	--enable-icall-export \
	--disable-icall-tables \
	--with-bitcode=yes

EMSCRIPTEN_VERSION=1.38.11
EMSCRIPTEN_SDK_DIR=$(TOP)/sdks/builds/toolchains/emsdk

$(TOP)/sdks/builds/toolchains/emsdk:
	git clone https://github.com/juj/emsdk.git $(EMSCRIPTEN_SDK_DIR)
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk install sdk-$(EMSCRIPTEN_VERSION)-64bit
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk activate --embedded sdk-$(EMSCRIPTEN_VERSION)-64bit
	-cd $(TOP)/sdks/builds/toolchains/emsdk/emscripten/$(EMSCRIPTEN_VERSION) && patch -p1 < $(TOP)/sdks/builds/emsdk-eh.diff

.stamp-wasm-toolchain: | $(TOP)/sdks/builds/toolchains/emsdk
	touch $@

.stamp-wasm-runtime-toolchain: .stamp-wasm-toolchain
	touch $@

.stamp-wasm-runtime-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/wasm-runtime
	cd $(TOP)/sdks/builds/wasm-runtime && source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && CFLAGS="-Os -g" emconfigure $(TOP)/configure $(WASM_RUNTIME_AC_VARS) $(WASM_RUNTIME_CONFIGURE_FLAGS)
	touch $@

build-custom-wasm-runtime:
	source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && make -C wasm-runtime

.PHONY: package-wasm-runtime
package-wasm-runtime:
	$(MAKE) -C $(TOP)/sdks/builds/wasm-runtime/mono install

.PHONY: clean-wasm
clean-wasm:
	rm -rf .stamp-wasm-runtime-toolchain $(TOP)/sdks/builds/toolchains/emsdk

.PHONY: clean-wasm-runtime
clean-wasm-runtime:
	rm -rf .stamp-wasm-runtime-configure $(TOP)/sdks/builds/wasm-runtime $(TOP)/sdks/builds/wasm-runtime.config.cache $(TOP)/sdks/out/wasm-runtime

TARGETS += wasm-runtime

UNAME := $(shell uname -s)
ifeq ($(UNAME),Linux)
	CROSS_HOST=i386-unknown-linux
	CPP_SHARP_DIR=linux_64
endif
ifeq ($(UNAME),Darwin)
	CROSS_HOST=i386-apple-darwin10
	CPP_SHARP_DIR=osx_64
endif

WASM_CROSS_CONFIGURE_FLAGS = \
	$(if $(ENABLE_CXX),--enable-cxx) \
	--cache-file=$(TOP)/sdks/builds/wasm-cross.config.cache \
	--prefix=$(TOP)/sdks/out/wasm-cross \
	--host=$(CROSS_HOST)	\
	--target=wasm32	\
	--disable-mcs-build \
	--disable-nls \
	--disable-boehm \
	--disable-btls \
	--disable-support-build \
	--enable-maintainer-mode	\
	--with-llvm=$(TOP)/sdks/out/llvm-llvm32 \
	--enable-minimal=appdomains,com,remoting \
	--with-cross-offsets=wasm-offsets.h

.stamp-wasm-cross-toolchain: .stamp-wasm-toolchain
	touch $@

.stamp-wasm-cross-configure: | $(TOP)/configure provision-llvm-llvm32
	mkdir -p $(TOP)/sdks/builds/wasm-cross
	cd $(TOP)/sdks/builds/wasm-cross && CFLAGS="-g -m32" CXXFLAGS="-g -m32" $(TOP)/configure $(WASM_CROSS_CONFIGURE_FLAGS)
	touch $@

# This needs to be run after the target runtime has been configured
$(TOP)/sdks/builds/wasm-cross/wasm-offsets.h: .stamp-wasm-cross-configure .stamp-wasm-runtime-configure $(TOP)/tools/offsets-tool/MonoAotOffsetsDumper.exe
	cd $(TOP)/sdks/builds/wasm-cross && MONO_PATH=$(TOP)/tools/offsets-tool/CppSharp/$(CPP_SHARP_DIR) mono --debug $(TOP)/tools/offsets-tool/MonoAotOffsetsDumper.exe --emscripten-sdk=$(EMSCRIPTEN_SDK_DIR)/emscripten/$(EMSCRIPTEN_VERSION) --abi wasm32-unknown-unknown --outfile $@ --mono $(TOP) --targetdir $(TOP)/sdks/builds/wasm-runtime

build-wasm-cross: $(TOP)/sdks/builds/wasm-cross/wasm-offsets.h

.PHONY: package-wasm-cross
package-wasm-cross:
	$(MAKE) -C $(TOP)/sdks/builds/wasm-cross/mono install

.PHONY: clean-wasm-cross
clean-wasm-cross:
	rm -rf .stamp-wasm-aot-toolchain .stamp-wasm-cross-configure $(TOP)/sdks/builds/wasm-cross $(TOP)/sdks/builds/wasm-cross.config.cache $(TOP)/sdks/out/wasm-cross

TARGETS += wasm-cross
