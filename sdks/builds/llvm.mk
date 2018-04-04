
LLVM_SRC?=$(TOP)/sdks/builds/toolchains/llvm

$(TOP)/sdks/builds/toolchains/llvm:
	git clone -b master https://github.com/mono/llvm.git $@
	cd $@ && git checkout $(LLVM_HASH)

$(LLVM_SRC)/configure: | $(LLVM_SRC)

##
# Parameters
#  $(1): target
#  $(2): arch
define LLVMTemplate

_llvm_$(1)_CXXFLAGS= \
	-stdlib=libc++ \
	-mmacosx-version-min=10.9

_llvm_$(1)_LDFLAGS= \
	-mmacosx-version-min=10.9

_llvm_$(1)_CONFIGURE_ENVIRONMENT= \
	CXXFLAGS="$$(_llvm_$(1)_CXXFLAGS)" \
	LDFLAGS="$$(_llvm_$(1)_LDFLAGS)"

_llvm_$(1)_CONFIGURE_FLAGS= \
	--host=$(2)-$$(if $$(filter $$(UNAME),Darwin),apple-darwin10,$$(if $$(filter $$(UNAME),Linux),linux-gnu,$$(error "Unknown UNAME='$$(UNAME)'"))) \
	--cache-file=$$(TOP)/sdks/builds/llvm-$(1).config.cache \
	--prefix=$$(TOP)/sdks/out/llvm-$(1) \
	--enable-assertions=no \
	--enable-optimized \
	--enable-targets="arm,aarch64,x86" \
	--enable-libcpp

.stamp-llvm-$(1)-toolchain: | $$(LLVM_SRC)
	touch $$@

.stamp-llvm-$(1)-configure: $$(LLVM_SRC)/configure
	mkdir -p $$(TOP)/sdks/builds/llvm-$(1)
	cd $$(TOP)/sdks/builds/llvm-$(1) && $$< $$(_llvm_$(1)_CONFIGURE_ENVIRONMENT) $$(_llvm_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: package-llvm-$(1)
package-llvm-$(1):
	$$(MAKE) -C $$(TOP)/sdks/builds/llvm-$(1) install

.PHONY: clean-llvm-$(1)
clean-llvm-$(1):
	rm -rf .stamp-llvm-$(1)-toolchain .stamp-llvm-$(1)-configure $$(TOP)/sdks/builds/llvm-$(1) $$(TOP)/sdks/builds/llvm-$(1).config.cache $$(TOP)/sdks/out/llvm-$(1)

TARGETS += llvm-$(1)

endef

$(eval $(call LLVMTemplate,llvm32,i386))
$(eval $(call LLVMTemplate,llvm64,x86_64))

##
# Parameters
#  $(1): target
#  $(2): arch
#
# Flags
#  llvm_$(1)_CONFIGURE_ENVIRONMENT
define LLVMMxeTemplate

_llvm_$(1)_PATH=$$(MXE_PREFIX)/bin

_llvm_$(1)_AR=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-ar
_llvm_$(1)_AS=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-as
_llvm_$(1)_CC=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-gcc
_llvm_$(1)_CXX=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-g++
_llvm_$(1)_DLLTOOL=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-dlltool
_llvm_$(1)_LD=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-ld
_llvm_$(1)_OBJDUMP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-objdump
_llvm_$(1)_RANLIB=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-ranlib
_llvm_$(1)_STRIP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-strip

_llvm_$(1)_CXXFLAGS=

_llvm_$(1)_LDFLAGS=

_llvm_$(1)_CONFIGURE_ENVIRONMENT = \
	AR="$$(_llvm_$(1)_AR)" \
	AS="$$(_llvm_$(1)_AS)" \
	CC="$$(_llvm_$(1)_CC)" \
	CXX="$$(_llvm_$(1)_CXX)" \
	DLLTOOL="$$(_llvm_$(1)_DLLTOOL)" \
	LD="$$(_llvm_$(1)_LD)" \
	OBJDUMP="$$(_llvm_$(1)_OBJDUMP)" \
	RANLIB="$$(_llvm_$(1)_RANLIB)" \
	STRIP="$$(_llvm_$(1)_STRIP)" \
	CXXFLAGS="$$(_llvm_$(1)_CXXFLAGS)" \
	LDFLAGS="$$(_llvm_$(1)_LDFLAGS)"

_llvm_$(1)_CONFIGURE_FLAGS = \
	--host=$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static) \
	--cache-file=$$(TOP)/sdks/builds/llvm-$(1).config.cache \
	--prefix=$$(TOP)/sdks/out/llvm-$(1) \
	--enable-assertions=no \
	--enable-optimized \
	--enable-targets="arm,aarch64,x86" \
	--disable-pthreads \
	--disable-zlib

.stamp-llvm-$(1)-toolchain: | $$(LLVM_SRC)
	touch $$@

.stamp-llvm-$(1)-configure: $$(LLVM_SRC)/configure | $(if $(IGNORE_PROVISION_MXE),,provision-mxe)
	mkdir -p $$(TOP)/sdks/builds/llvm-$(1)
	cd $$(TOP)/sdks/builds/llvm-$(1) && PATH="$$$$PATH:$$(_llvm_$(1)_PATH)" $$< $$(_llvm_$(1)_CONFIGURE_ENVIRONMENT) $$(_llvm_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: package-llvm-$(1)
package-llvm-$(1):
	$$(MAKE) -C $$(TOP)/sdks/builds/llvm-$(1) install

.PHONY: clean-llvm-$(1)
clean-llvm-$(1):
	rm -rf .stamp-llvm-$(1)-toolchain .stamp-llvm-$(1)-configure $$(TOP)/sdks/builds/llvm-$(1) $$(TOP)/sdks/builds/llvm-$(1).config.cache $$(TOP)/sdks/out/llvm-$(1)

TARGETS += llvm-$(1)

endef

ifneq ($(MXE_PREFIX),)
$(eval $(call LLVMMxeTemplate,llvmwin32,i686))
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64))
endif
