#emcc has lots of bash'isms
SHELL:=/bin/bash

EMSCRIPTEN_VERSION=1.38.11
EMSCRIPTEN_SDK_DIR=$(TOP)/sdks/builds/toolchains/emsdk

$(TOP)/sdks/builds/toolchains/emsdk:
	git clone https://github.com/juj/emsdk.git $(EMSCRIPTEN_SDK_DIR)
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk install sdk-$(EMSCRIPTEN_VERSION)-64bit
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk activate --embedded sdk-$(EMSCRIPTEN_VERSION)-64bit
	-cd $(TOP)/sdks/builds/toolchains/emsdk/emscripten/$(EMSCRIPTEN_VERSION) && patch -p1 < $(TOP)/sdks/builds/emsdk-eh.diff

.PHONY: provision-wasm
provision-wasm: | $(EMSCRIPTEN_SDK_DIR)

WASM_RUNTIME_AC_VARS= \
	ac_cv_func_shm_open_working_with_mmap=no

WASM_RUNTIME_CONFIGURE_FLAGS = \
	--cache-file=$(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION).config.cache \
	--prefix=$(TOP)/sdks/out/wasm-runtime-$(CONFIGURATION) \
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
	--with-bitcode=yes \
	$(if $(ENABLE_CXX),--enable-cxx)

.stamp-wasm-runtime-toolchain:
	touch $@

.stamp-wasm-runtime-$(CONFIGURATION)-configure: $(TOP)/configure $(if $(IGNORE_PROVISION_WASM),,provision-wasm)
	mkdir -p $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION)
	cd $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION) && source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && CFLAGS="-Os -g" emconfigure $(TOP)/configure $(WASM_RUNTIME_AC_VARS) $(WASM_RUNTIME_CONFIGURE_FLAGS)
	touch $@

.PHONY: .stamp-wasm-runtime-configure
.stamp-wasm-runtime-configure: .stamp-wasm-runtime-$(CONFIGURATION)-configure

.PHONY: build-custom-wasm-runtime
build-custom-wasm-runtime:
	source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && $(MAKE) -C wasm-runtime-$(CONFIGURATION)

.PHONY: setup-custom-wasm-runtime
setup-custom-wasm-runtime:
	mkdir -p $(TOP)/sdks/out/wasm-runtime-$(CONFIGURATION)

.PHONY: package-wasm-runtime
package-wasm-runtime:
	$(MAKE) -C $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION)/mono install

.PHONY: clean-wasm-runtime
clean-wasm-runtime:
	rm -rf .stamp-wasm-runtime-toolchain .stamp-wasm-runtime-$(CONFIGURATION)-configure $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION) $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION).config.cache $(TOP)/sdks/out/wasm-runtime-$(CONFIGURATION)

$(eval $(call TargetTemplate,wasm,runtime))

.PHONY: archive-wasm
archive-wasm: package-wasm-runtime

wasm_ARCHIVE += wasm-runtime-$(CONFIGURATION)

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define WasmCrossTemplate

_wasm-$(1)_OFFSETS_DUMPER_ARGS=--emscripten-sdk="$$(EMSCRIPTEN_SDK_DIR)/emscripten/$$(EMSCRIPTEN_VERSION)"

_wasm-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-mcs-build \
	--disable-nls \
	--disable-support-build \
	--enable-maintainer-mode \
	--enable-minimal=appdomains,com,remoting

$$(eval $$(call CrossRuntimeTemplate,wasm,$(1),$$(if $$(filter $$(UNAME),Darwin),$(2)-apple-darwin10,$$(if $$(filter $$(UNAME),Linux),$(2)-linux-gnu,$$(error "Unknown UNAME='$$(UNAME)'"))),$(3)-unknown-none,$(4),$(5),$(6)))

endef

# 64 bit cross compiler
$(eval $(call WasmCrossTemplate,cross,x86_64,wasm32,runtime,llvm-llvm64,wasm32-unknown-unknown))
# Old 32 bit cross compiler
$(eval $(call WasmCrossTemplate,cross-32,i686,wasm32,runtime,llvm-llvm32,wasm32-unknown-unknown))

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define WasmCrossMXETemplate

_wasm-$(1)_OFFSETS_DUMPER_ARGS=--emscripten-sdk="$(EMSCRIPTEN_SDK_DIR)/emscripten/$(EMSCRIPTEN_VERSION)"

_wasm-$(1)_PATH=$$(MXE_PREFIX)/bin

_wasm-$(1)_AR=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-ar
_wasm-$(1)_AS=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-as
_wasm-$(1)_CC=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-gcc
_wasm-$(1)_CXX=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-g++
_wasm-$(1)_DLLTOOL=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-dlltool
_wasm-$(1)_LD=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-ld
_wasm-$(1)_OBJDUMP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-objdump
_wasm-$(1)_RANLIB=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-ranlib
_wasm-$(1)_STRIP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $$(UNAME),Darwin),.static)-strip

_wasm-$(1)_CFLAGS= \
	$$(if $$(RELEASE),,-DDEBUG_CROSS) \
	-static \
	-static-libgcc

_wasm-$(1)_CXXFLAGS= \
	$$(if $$(RELEASE),,-DDEBUG_CROSS) \
	-static \
	-static-libgcc

_wasm-$(1)_LDFLAGS= \
	-static \
	-static-libgcc \
	-static-libstdc++

_wasm-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-mcs-build \
	--disable-nls \
	--disable-nls \
	--disable-support-build \
	--enable-maintainer-mode \
	--enable-minimal=appdomains,com,remoting \
	--with-tls=pthread

.stamp-wasm-$(1)-$$(CONFIGURATION)-configure: | $$(if $$(IGNORE_PROVISION_MXE),,provision-mxe)

$$(eval $$(call CrossRuntimeTemplate,wasm,$(1),$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static),$(3)-unknown-none,$(4),$(5),$(6)))

endef


$(eval $(call WasmCrossMXETemplate,cross-win,i686,wasm32,runtime,llvm-llvmwin32,wasm32-unknown-unknown))

$(eval $(call BclTemplate,wasm,wasm wasm_tools,wasm))
