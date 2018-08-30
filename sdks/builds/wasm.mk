#emcc has lots of bash'isms
SHELL:=/bin/bash

WASM_INTERP_CONFIGURE_FLAGS = \
	--cache-file=$(TOP)/sdks/builds/wasm-interp.config.cache \
	--prefix=$(TOP)/sdks/out/wasm-interp \
	--enable-interpreter \
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
	--enable-minimal=ssa,com,jit,reflection_emit_save,reflection_emit,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_marksweep_conc,sgen_split_nursery,sgen_gc_bridge,logging,remoting,shared_perfcounters,sgen_debug_helpers,soft_debug \
	--host=wasm32


$(TOP)/sdks/builds/toolchains/emsdk:
	git clone https://github.com/juj/emsdk.git $(TOP)/sdks/builds/toolchains/emsdk

.stamp-wasm-toolchain: | $(TOP)/sdks/builds/toolchains/emsdk
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk install sdk-1.38.11-64bit
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk activate --embedded sdk-1.38.11-64bit
	touch $@

.stamp-wasm-interp-toolchain: .stamp-wasm-toolchain
	touch $@

.stamp-wasm-interp-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/wasm-interp
	cd $(TOP)/sdks/builds/wasm-interp && source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && CFLAGS="-Os -g" emconfigure $(TOP)/configure $(WASM_INTERP_CONFIGURE_FLAGS)
	touch $@

build-custom-wasm-interp:
	source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && make -C wasm-interp

.PHONY: package-wasm-interp
package-wasm-interp:
	$(MAKE) -C $(TOP)/sdks/builds/wasm-interp/mono install

.PHONY: clean-wasm
clean-wasm:
	rm -rf .stamp-wasm-toolchain $(TOP)/sdks/builds/toolchains/emsdk

.PHONY: clean-wasm-interp
clean-wasm-interp: clean-wasm
	rm -rf .stamp-wasm-interp-toolchain .stamp-wasm-interp-configure $(TOP)/sdks/builds/wasm-interp $(TOP)/sdks/builds/wasm.config.cache $(TOP)/sdks/out/wasm-interp

TARGETS += wasm-interp

UNAME := $(shell uname -s)
ifeq ($(UNAME),Linux)
	CROSS_HOST=i386-unknown-linux
endif
ifeq ($(UNAME),Darwin)
	CROSS_HOST=i386-apple-darwin10
endif

WASM_AOT_CONFIGURE_FLAGS = \
	--cache-file=$(TOP)/sdks/builds/wasm-aot.config.cache \
	--prefix=$(TOP)/sdks/out/wasm-aot \
	--host=$(CROSS_HOST)	\
	--target=wasm32	\
	--disable-mcs-build \
	--disable-nls \
	--disable-boehm \
	--disable-btls \
	--disable-support-build \
	--enable-maintainer-mode	\
	--enable-llvm	\
	--enable-minimal=appdomains,com,remoting

.stamp-wasm-aot-toolchain: .stamp-wasm-toolchain
	touch $@

.stamp-wasm-aot-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/wasm-aot
	cd $(TOP)/sdks/builds/wasm-aot && CFLAGS="-g" $(TOP)/configure $(WASM_AOT_CONFIGURE_FLAGS)
	touch $@

.PHONY: package-wasm-aot
package-wasm-aot:
	$(MAKE) -C $(TOP)/sdks/builds/wasm-aot/mono install

.PHONY: clean-wasm-aot
clean-wasm-aot: clean-wasm
	rm -rf .stamp-wasm-aot-toolchain .stamp-wasm-aot-configure $(TOP)/sdks/builds/wasm-aot $(TOP)/sdks/builds/wasm.config.cache $(TOP)/sdks/out/wasm-aot

TARGETS += wasm-aot


WASM_AOT_RUNTIME_CONFIGURE_FLAGS = \
	--cache-file=$(TOP)/sdks/builds/wasm-aot-runtime.config.cache \
	--prefix=$(TOP)/sdks/out/wasm-aot-runtime \
	--enable-llvm-runtime \
	--with-bitcode=yes	\
	--host=wasm32	\
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
	--enable-minimal=ssa,com,jit,reflection_emit_save,reflection_emit,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_marksweep_conc,sgen_split_nursery,sgen_gc_bridge,logging,remoting,shared_perfcounters,sgen_debug_helpers,soft_debug

.stamp-wasm-aot-runtime-toolchain: .stamp-wasm-toolchain
	touch $@

.stamp-wasm-aot-runtime-configure: $(TOP)/configure
	mkdir -p $(TOP)/sdks/builds/wasm-aot-runtime
	cd $(TOP)/sdks/builds/wasm-aot-runtime && source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && CFLAGS="-Os -g" emconfigure $(TOP)/configure $(WASM_AOT_RUNTIME_CONFIGURE_FLAGS)
	touch $@

build-custom-wasm-aot-runtime:
	source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && make -C wasm-aot-runtime

.PHONY: package-wasm-aot-runtime
package-wasm-aot-runtime:
	$(MAKE) -C $(TOP)/sdks/builds/wasm-aot-runtime/mono install

.PHONY: clean-wasm-aot-runtime
clean-wasm-aot-runtime: clean-wasm
	rm -rf .stamp-wasm-aot-runtime-toolchain .stamp-wasm-aot-runtime-configure $(TOP)/sdks/builds/wasm-aot-runtime $(TOP)/sdks/builds/wasm.config.cache $(TOP)/sdks/out/wasm-aot-runtime

TARGETS += wasm-aot-runtime