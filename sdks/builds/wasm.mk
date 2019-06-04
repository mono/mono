#emcc has lots of bash'isms
SHELL:=/bin/bash

EMSCRIPTEN_VERSION=1.38.34
EMSCRIPTEN_SDK_DIR=$(TOP)/sdks/builds/toolchains/emsdk

MONO_SUPPORT=$(TOP)/support

ZLIB_HEADERS = \
	$(MONO_SUPPORT)/crc32.h		\
	$(MONO_SUPPORT)/deflate.h  	\
	$(MONO_SUPPORT)/inffast.h  	\
	$(MONO_SUPPORT)/inffixed.h  	\
	$(MONO_SUPPORT)/inflate.h  	\
	$(MONO_SUPPORT)/inftrees.h  	\
	$(MONO_SUPPORT)/trees.h  	\
	$(MONO_SUPPORT)/zconf.h  	\
	$(MONO_SUPPORT)/zlib.h  	\
	$(MONO_SUPPORT)/zutil.h

$(TOP)/sdks/builds/toolchains/emsdk:
	git clone https://github.com/juj/emsdk.git $(EMSCRIPTEN_SDK_DIR)

.stamp-wasm-checkout-and-update-emsdk: | $(EMSCRIPTEN_SDK_DIR)
	cd $(TOP)/sdks/builds/toolchains/emsdk && git reset --hard && git clean -xdff && git pull
	touch $@

#This is a weird rule to workaround the circularity of the next rule.
#.stamp-wasm-install-and-select-$(EMSCRIPTEN_VERSION) depends on .emscripten and, at the same time, it updates it.
#This is designed to force the .stamp target to rerun when a different emscripten version is selected, which causes .emscripten to be updated
$(EMSCRIPTEN_SDK_DIR)/.emscripten: | $(EMSCRIPTEN_SDK_DIR)
	touch $@

.stamp-wasm-install-and-select-$(EMSCRIPTEN_VERSION): .stamp-wasm-checkout-and-update-emsdk $(EMSCRIPTEN_SDK_DIR)/.emscripten
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk install $(EMSCRIPTEN_VERSION)-upstream
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk activate --embedded $(EMSCRIPTEN_VERSION)-upstream
	cd $(TOP)/sdks/builds/toolchains/emsdk/upstream/emscripten && (patch -N -p1 < $(TOP)/sdks/builds/fix-emscripten-8511.diff; exit 0)
	touch $@

.PHONY: provision-wasm
provision-wasm: .stamp-wasm-install-and-select-$(EMSCRIPTEN_VERSION)

WASM_RUNTIME_AC_VARS= \
	ac_cv_func_shm_open_working_with_mmap=no

WASM_RUNTIME_CFLAGS=-fexceptions $(if $(RELEASE),-Os -g,-O0 -ggdb3 -fno-omit-frame-pointer)
WASM_RUNTIME_CXXFLAGS=$(WASM_RUNTIME_CFLAGS) -s DISABLE_EXCEPTION_CATCHING=0

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
	--enable-minimal=ssa,com,jit,reflection_emit_save,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_marksweep_conc,sgen_split_nursery,sgen_gc_bridge,logging,remoting,shared_perfcounters,sgen_debug_helpers,soft_debug,interpreter,assert_messages,cleanup,mdb \
	--host=wasm32 \
	--enable-llvm-runtime \
	--enable-icall-export \
	--disable-icall-tables \
	--disable-crash-reporting \
	--with-bitcode=yes \
	$(if $(ENABLE_CXX),--enable-cxx) \
	CFLAGS="$(WASM_RUNTIME_CFLAGS)" \
	CXXFLAGS="$(WASM_RUNTIME_CXXFLAGS)" \

.stamp-wasm-runtime-toolchain:
	touch $@

.stamp-wasm-runtime-$(CONFIGURATION)-configure: $(TOP)/configure | $(if $(IGNORE_PROVISION_WASM),,provision-wasm)
	mkdir -p $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION)
	cd $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION) && source $(TOP)/sdks/builds/toolchains/emsdk/emsdk_env.sh && emconfigure $(TOP)/configure $(WASM_RUNTIME_AC_VARS) $(WASM_RUNTIME_CONFIGURE_FLAGS)
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
	# We do not build the support library but we will use the zlib headers to activate
	# zlib support for wasm through emscripten.  See flag "-s USE_ZLIB=1" in wasm build
	mkdir -p $(TOP)/sdks/out/wasm-runtime-$(CONFIGURATION)/include/support
	cp -r $(ZLIB_HEADERS) $(TOP)/sdks/out/wasm-runtime-$(CONFIGURATION)/include/support/

.PHONY: clean-wasm-runtime
clean-wasm-runtime:
	rm -rf .stamp-wasm-runtime-toolchain .stamp-wasm-runtime-$(CONFIGURATION)-configure $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION) $(TOP)/sdks/builds/wasm-runtime-$(CONFIGURATION).config.cache $(TOP)/sdks/out/wasm-runtime-$(CONFIGURATION)

$(eval $(call TargetTemplate,wasm,runtime))

.PHONY: configure-wasm
configure-wasm: configure-wasm-runtime

.PHONY: build-wasm
build-wasm: build-wasm-runtime

.PHONY: archive-wasm
archive-wasm: package-wasm-runtime

wasm_ARCHIVE += wasm-runtime-$(CONFIGURATION)

ifeq ($(UNAME),Darwin)
# The c# offsets tool is 32 bit, and the 64 bit version doesn't work
USE_OFFSETS_TOOL_PY = 1
endif

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define WasmCrossTemplate

_wasm-$(1)_OFFSETS_DUMPER_ARGS=--emscripten-sdk="$$(EMSCRIPTEN_SDK_DIR)/upstream/emscripten"

_wasm-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-mcs-build \
	--disable-nls \
	--disable-support-build \
	--enable-maintainer-mode \
	--enable-minimal=appdomains,com,remoting \
	--enable-icall-symbol-map \
	--with-cooperative-gc=no \
	--enable-hybrid-suspend=no \
	--with-cross-offsets=wasm32-unknown-none.h

$$(eval $$(call CrossRuntimeTemplate,wasm,$(1),$$(if $$(filter $$(UNAME),Darwin),$(2)-apple-darwin10,$$(if $$(filter $$(UNAME),Linux),$(2)-linux-gnu,$$(error "Unknown UNAME='$$(UNAME)'"))),$(3)-unknown-none,$(4),$(5),$(6)))

endef

# 64 bit cross compiler
$(eval $(call WasmCrossTemplate,cross,x86_64,wasm32,runtime,llvm-llvm64,wasm32-unknown-unknown))

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define WasmCrossMXETemplate

_wasm-$(1)_OFFSETS_DUMPER_ARGS=--emscripten-sdk="$(EMSCRIPTEN_SDK_DIR)/upstream/emscripten"

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
	-static-libgcc \
	-static-libstdc++

_wasm-$(1)_LDFLAGS= \
	-static \
	-static-libgcc

_wasm-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-mcs-build \
	--disable-nls \
	--disable-nls \
	--disable-support-build \
	--enable-maintainer-mode \
	--enable-minimal=appdomains,com,remoting \
	--with-tls=pthread \
	--enable-icall-symbol-map \
	--with-cross-offsets=wasm32-unknown-none.h

.stamp-wasm-$(1)-$$(CONFIGURATION)-configure: | $$(if $$(IGNORE_PROVISION_MXE),,provision-mxe)

$$(eval $$(call CrossRuntimeTemplate,wasm,$(1),$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static),$(3)-unknown-none,$(4),$(5),$(6)))

endef

$(eval $(call WasmCrossMXETemplate,cross-win,i686,wasm32,runtime,llvm-llvmwin32,wasm32-unknown-unknown))

$(eval $(call BclTemplate,wasm,wasm wasm_tools,wasm))
