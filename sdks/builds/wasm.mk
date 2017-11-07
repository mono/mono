
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
	--enable-minimal=ssa,com,jit,reflection_emit_save,reflection_emit,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging,remoting,shared_perfcounters \
	--host=i386-apple-darwin10

#toolchain code
.stamp-wasm-toolchain:
	git clone https://github.com/juj/emsdk.git $(TOP)/sdks/builds/toolchains/emsdk
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk install latest && ./emsdk activate --embedded latest
	touch $@

.stamp-wasm-interp-toolchain: .stamp-wasm-toolchain
	touch $@

#configure step
.stamp-wasm-interp-configure: $(TOP)/configure .stamp-wasm-interp-toolchain
	mkdir -p $(TOP)/sdks/builds/wasm-interp
	cd $(TOP)/sdks/builds/wasm-interp && source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && CFLAGS="-Os -g" emconfigure $(TOP)/configure $(WASM_INTERP_CONFIGURE_FLAGS)
	touch $@

package-wasm-interp:
	$(MAKE) -C $(TOP)/sdks/builds/wasm-interp/mono install


#custom build rule
CUSTOM_BUILD_TARGETS += build-wasm-interp
build-wasm-interp: .stamp-wasm-interp-configure
	source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && make -C wasm-interp



.PHONY: clean-wasm-interp clean-wasm
clean-wasm::
	rm -rf .stamp-wasm-toolchain
clean-wasm-interp:: clean-wasm
	rm -rf .stamp-wasm-configure $(TOP)/sdks/builds/toolchains/emcc $(TOP)/sdks/builds/wasm $$(TOP)/sdks/builds/wasm.config.cache

TARGETS += wasm-interp



