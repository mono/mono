
$(TOP)/sdks/builds/toolchains/llvm:
	git clone -b master https://github.com/mono/llvm.git $@

$(TOP)/sdks/builds/toolchains/llvm/configure: | $(TOP)/sdks/builds/toolchains/llvm

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
	--host=$(2)-apple-darwin10 \
	--cache-file=$$(TOP)/sdks/builds/llvm-$(1).config.cache \
	--prefix=$$(TOP)/sdks/out/llvm-$(1) \
	--enable-assertions=no \
	--enable-optimized \
	--enable-targets="arm,aarch64,x86" \
	--enable-libcpp

.stamp-llvm-$(1)-toolchain: | $$(TOP)/sdks/builds/toolchains/llvm
	touch $$@

.stamp-llvm-$(1)-configure: $$(TOP)/sdks/builds/toolchains/llvm/configure
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

_llvm_$(1)_PATH=$$(TOP)/sdks/out/mxe/bin

_llvm_$(1)_AR=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-ar
_llvm_$(1)_AS=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-as
_llvm_$(1)_CC=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-gcc
_llvm_$(1)_CXX=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-g++
_llvm_$(1)_DLLTOOL=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-dlltool
_llvm_$(1)_LD=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-ld
_llvm_$(1)_OBJDUMP=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-objdump
_llvm_$(1)_RANLIB=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-ranlib
_llvm_$(1)_STRIP=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-strip

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
	--host=$(2)-w64-mingw32.static \
	--cache-file=$$(TOP)/sdks/builds/llvm-$(1).config.cache \
	--prefix=$$(TOP)/sdks/out/llvm-$(1) \
	--enable-assertions=no \
	--enable-optimized \
	--enable-targets="arm,aarch64,x86" \
	--disable-pthreads \
	--disable-zlib

.stamp-llvm-$(1)-toolchain: | $$(TOP)/sdks/builds/toolchains/llvm
	cd $$(TOP)/sdks/builds/toolchains/llvm && git checkout $(LLVM_HASH)
	touch $$@

.stamp-llvm-$(1)-configure: $$(TOP)/sdks/builds/toolchains/llvm/configure | package-mxe
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

$(eval $(call LLVMMxeTemplate,llvmwin32,i686))
$(eval $(call LLVMMxeTemplate,llvmwin64,x86_64))
