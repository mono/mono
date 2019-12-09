#emcc has lots of bash'isms
SHELL:=/bin/bash

EMSCRIPTEN_VERSION=1.39.4
EMSCRIPTEN_LOCAL_SDK_DIR=$(TOP)/sdks/builds/toolchains/emsdk

EMSCRIPTEN_SDK_DIR ?= $(EMSCRIPTEN_LOCAL_SDK_DIR)

ifeq ($(UNAME),Darwin)
WASM_LIBCLANG=$(EMSCRIPTEN_SDK_DIR)/upstream/lib/libclang.dylib
else ifeq ($(UNAME),Linux)
WASM_LIBCLANG=$(EMSCRIPTEN_SDK_DIR)/upstream/lib/libclang.so
endif

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
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk install $(EMSCRIPTEN_VERSION)
	cd $(TOP)/sdks/builds/toolchains/emsdk && ./emsdk activate --embedded $(EMSCRIPTEN_VERSION)
	cd $(TOP)/sdks/builds/toolchains/emsdk/upstream/emscripten && (patch -N -p1 < $(TOP)/sdks/builds/fix-emscripten-8511.diff; exit 0)
	cd $(TOP)/sdks/builds/toolchains/emsdk/upstream/emscripten && (patch -N -p1 < $(TOP)/sdks/builds/emscripten-pr-8457.diff; exit 0)
	touch $@

.PHONY: provision-wasm

ifeq ($(EMSCRIPTEN_SDK_DIR),$(EMSCRIPTEN_LOCAL_SDK_DIR))
provision-wasm: .stamp-wasm-install-and-select-$(EMSCRIPTEN_VERSION)
else
provision-wasm:
endif

WASM_RUNTIME_AC_VARS= \
	ac_cv_func_shm_open_working_with_mmap=no

WASM_RUNTIME_BASE_CFLAGS=-fexceptions $(if $(RELEASE),-Os -g,-O0 -ggdb3 -fno-omit-frame-pointer)
WASM_RUNTIME_BASE_CXXFLAGS=$(WASM_RUNTIME_BASE_CFLAGS) -s DISABLE_EXCEPTION_CATCHING=0

WASM_DISABLED_FEATURES=ssa,com,jit,reflection_emit_save,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_marksweep_conc,sgen_split_nursery,sgen_gc_bridge,logging,remoting,shared_perfcounters,sgen_debug_helpers,soft_debug,interpreter,assert_messages,cleanup,mdb,gac

WASM_RUNTIME_BASE_CONFIGURE_FLAGS = \
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
	--host=wasm32 \
	--enable-llvm-runtime \
	--enable-icall-export \
	--disable-icall-tables \
	--disable-crash-reporting \
	--with-bitcode=yes \
	$(if $(ENABLE_CXX),--enable-cxx)

# $(1) - target
define WasmRuntimeTemplate

_wasm_$(1)_CONFIGURE_FLAGS = \
	$(WASM_RUNTIME_BASE_CONFIGURE_FLAGS) \
	--enable-minimal=$(WASM_DISABLED_FEATURES)$$(wasm_$(1)_DISABLED_FEATURES) \
	--cache-file=$(TOP)/sdks/builds/wasm-$(1)-$(CONFIGURATION).config.cache \
	--prefix=$(TOP)/sdks/out/wasm-$(1)-$(CONFIGURATION) \
	$$(wasm_$(1)_CONFIGURE_FLAGS) \
	CFLAGS="$(WASM_RUNTIME_BASE_CFLAGS) $$(wasm_$(1)_CFLAGS)" \
	CXXFLAGS="$(WASM_RUNTIME_BASE_CXXFLAGS) $$(wasm_$(1)_CXXFLAGS)"

.stamp-wasm-$(1)-toolchain:
	touch $$@

.stamp-wasm-$(1)-$(CONFIGURATION)-configure: $(TOP)/configure | $(if $(IGNORE_PROVISION_WASM),,provision-wasm)
	mkdir -p $(TOP)/sdks/builds/wasm-$(1)-$(CONFIGURATION)
	cd $(TOP)/sdks/builds/wasm-$(1)-$(CONFIGURATION) && source $(EMSCRIPTEN_SDK_DIR)/emsdk_env.sh && emconfigure $(TOP)/configure $(WASM_RUNTIME_AC_VARS) $$(_wasm_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: .stamp-wasm-$(1)-configure
.stamp-wasm-$(1)-configure: .stamp-wasm-$(1)-$(CONFIGURATION)-configure

.PHONY: build-custom-wasm-$(1)
build-custom-wasm-$(1):
	source $(EMSCRIPTEN_SDK_DIR)/emsdk_env.sh && $(MAKE) -C wasm-$(1)-$(CONFIGURATION)

.PHONY: setup-custom-wasm-$(1)
setup-custom-wasm-$(1):
	mkdir -p $(TOP)/sdks/out/wasm-$(1)-$(CONFIGURATION)

.PHONY: package-wasm-$(1)
package-wasm-$(1):
	source $(EMSCRIPTEN_SDK_DIR)/emsdk_env.sh && $(MAKE) -C $(TOP)/sdks/builds/wasm-$(1)-$(CONFIGURATION)/mono install

.PHONY: clean-wasm-$(1)
clean-wasm-$(1):
	rm -rf .stamp-wasm-$(1)-toolchain .stamp-wasm-$(1)-$(CONFIGURATION)-configure $(TOP)/sdks/builds/wasm-$(1)-$(CONFIGURATION) $(TOP)/sdks/out/wasm-$(1)-$(CONFIGURATION)
ifeq ($(KEEP_CONFIG_CACHE),)
	rm -rf $(TOP)/sdks/builds/wasm-$(1)-$(CONFIGURATION).config.cache
endif

clean-wasm-$(1)-cache:
	rm -rf $(TOP)/sdks/builds/wasm-$(1)-$(CONFIGURATION).config.cache

$(eval $(call TargetTemplate,wasm,$(1)))

.PHONY: configure-wasm
configure-wasm: configure-wasm-$(1)

.PHONY: build-wasm
build-wasm: build-wasm-$(1)

.PHONY: archive-wasm
archive-wasm: package-wasm-$(1)

wasm_ARCHIVE += wasm-$(1)-$(CONFIGURATION)

endef

wasm_runtime_DISABLED_FEATURES=,threads
wasm_runtime-netcore_DISABLED_FEATURES=,threads
wasm_runtime-netcore_CONFIGURE_FLAGS=--with-core=only
wasm_runtime-threads_CFLAGS=-s USE_PTHREADS=1 -pthread
wasm_runtime-threads_CXXFLAGS=-s USE_PTHREADS=1 -pthread

wasm_runtime-dynamic_CFLAGS=-s WASM_OBJECT_FILES=0
wasm_runtime-dynamic_CXXFLAGS=-s WASM_OBJECT_FILES=0

$(eval $(call WasmRuntimeTemplate,runtime))
ifdef ENABLE_WASM_THREADS
$(eval $(call WasmRuntimeTemplate,runtime-threads))
endif
ifdef ENABLE_WASM_DYNAMIC_RUNTIME
$(eval $(call WasmRuntimeTemplate,runtime-dynamic))
endif
ifdef ENABLE_WASM_NETCORE
$(eval $(call WasmRuntimeTemplate,runtime-netcore))
endif

WASM_CROSS_BASE_CONFIGURE_FLAGS= \
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

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define WasmCrossTemplate

_wasm-$(1)_OFFSETS_DUMPER_ARGS=--emscripten-sdk="$$(EMSCRIPTEN_SDK_DIR)/upstream/emscripten" --libclang="$$(WASM_LIBCLANG)"

_wasm-$(1)_CONFIGURE_FLAGS= \
	$(WASM_CROSS_BASE_CONFIGURE_FLAGS) \
	$$(wasm_$(1)_CONFIGURE_FLAGS)

$$(eval $$(call CrossRuntimeTemplate,wasm,$(1),$$(if $$(filter $$(UNAME),Darwin),$(2)-apple-darwin10,$$(if $$(filter $$(UNAME),Linux),$(2)-linux-gnu,$(2)-unknown)),$(3)-unknown-none,$(4),$(5),$(6)))

endef

wasm_cross-netcore_CONFIGURE_FLAGS=--with-core=only

$(eval $(call WasmCrossTemplate,cross,x86_64,wasm32,runtime,llvm-llvm64,wasm32-unknown-unknown))
ifdef ENABLE_WASM_NETCORE
$(eval $(call WasmCrossTemplate,cross-netcore,x86_64,wasm32,runtime,llvm-llvm64,wasm32-unknown-unknown))
endif

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define WasmCrossMXETemplate

_wasm-$(1)_OFFSETS_DUMPER_ARGS=--emscripten-sdk="$$(EMSCRIPTEN_SDK_DIR)/upstream/emscripten" --libclang="$$(WASM_LIBCLANG)"

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

ifdef ENABLE_WINDOWS
$(eval $(call WasmCrossMXETemplate,cross-win,x86_64,wasm32,runtime,llvm-llvmwin64,wasm32-unknown-unknown))
endif

$(eval $(call BclTemplate,wasm,wasm wasm_tools,wasm))

ifdef ENABLE_WASM_NETCORE

build-wasm-bcl-netcore: build-wasm-runtime-netcore
	in=wasm-runtime-netcore-release/netcore/config.make; out=../../netcore/config.make; if ! cmp -s $$in $$out ; then cp $$in $$out ; fi
	make -C ../../netcore

package-wasm-bcl-netcore: build-wasm-bcl-netcore
	mkdir -p ../out/wasm-bcl/netcore
	cp ../../netcore/System.Private.CoreLib/bin/wasm32/System.Private.CoreLib.{dll,pdb} ../out/wasm-bcl/netcore/

endif
