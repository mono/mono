
WASM_INTERP_CONFIGURE_FLAGS = \
	--cache-file=$(TOP)/sdks/builds/wasm-interp.config.cache \
	--prefix=$(TOP)/sdks/out/wasm-interp \
	--enable-wasm \
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
	--enable-minimal=ssa,com,jit,reflection_emit_save,reflection_emit,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_marksweep_conc,sgen_split_nursery,sgen_gc_bridge,logging,remoting,shared_perfcounters,sgen_debug_helpers \
	--host=i386-apple-darwin10

$(TOP)/sdks/builds/toolchains/emsdk:
	git clone https://github.com/juj/emsdk.git $(TOP)/sdks/builds/toolchains/emsdk

.stamp-wasm-toolchain: | $(TOP)/sdks/builds/toolchains/emsdk
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk install latest
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk activate --embedded latest
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
	rm -rf .stamp-wasm-interp-toolchain .stamp-wasm-interp-configure $(TOP)/sdks/builds/wasm $(TOP)/sdks/builds/wasm.config.cache $(TOP)/sdks/out/wasm-interp

TARGETS += wasm-interp



