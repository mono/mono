
##
# Parameters:
#  $(1): target
#  $(2): host triple
#
# Flags:
#  _$(1)_AR
#  _$(1)_AS
#  _$(1)_CC
#  _$(1)_CPP
#  _$(1)_CXX
#  _$(1)_CXXCPP
#  _$(1)_DLLTOOL
#  _$(1)_LD
#  _$(1)_OBJDUMP
#  _$(1)_RANLIB
#  _$(1)_STRIP
#  _$(1)_CFLAGS
#  _$(1)_CXXFLAGS
#  _$(1)_CPPFLAGS
#  _$(1)_LDFLAGS
#  _$(1)_AC_VARS
#  _$(1)_CONFIGURE_FLAGS
#  _$(1)_PATH
define RuntimeTemplate

_runtime_$(1)_BITNESS=$$(if $$(or $$(findstring i686,$(2)),$$(findstring i386,$(2))),-m32,$$(if $$(findstring x86_64,$(2)),-m64))

_runtime_$(1)_CFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CFLAGS) $$($(1)_CFLAGS) $$(_runtime_$(1)_BITNESS)
_runtime_$(1)_CXXFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CXXFLAGS) $$($(1)_CXXFLAGS) $$(_runtime_$(1)_BITNESS)
_runtime_$(1)_CPPFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CPPFLAGS) $$($(1)_CPPFLAGS) $$(_runtime_$(1)_BITNESS)
_runtime_$(1)_CXXCPPFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CXXCPPFLAGS) $$($(1)_CXXCPPFLAGS) $$(_runtime_$(1)_BITNESS)
_runtime_$(1)_LDFLAGS=$$(_$(1)_LDFLAGS) $$($(1)_LDFLAGS)

_runtime_$(1)_AC_VARS=$$(_$(1)_AC_VARS) $$($(1)_AC_VARS)

_runtime_$(1)_CONFIGURE_ENVIRONMENT = \
	$(if $$(_$(1)_AR),AR="$$(_$(1)_AR)") \
	$(if $$(_$(1)_AS),AS="$$(_$(1)_AS)") \
	$(if $$(_$(1)_CC),CC="$$(_$(1)_CC)") \
	$(if $$(_$(1)_CPP),CPP="$$(_$(1)_CPP)") \
	$(if $$(_$(1)_CXX),CXX="$$(_$(1)_CXX)") \
	$(if $$(_$(1)_CXXCPP),CXXCPP="$$(_$(1)_CXXCPP)") \
	$(if $$(_$(1)_DLLTOOL),DLLTOOL="$$(_$(1)_DLLTOOL)") \
	$(if $$(_$(1)_LD),LD="$$(_$(1)_LD)") \
	$(if $$(_$(1)_OBJDUMP),OBJDUMP="$$(_$(1)_OBJDUMP)") \
	$(if $$(_$(1)_RANLIB),RANLIB="$$(_$(1)_RANLIB)") \
	$(if $$(_$(1)_STRIP),STRIP="$$(_$(1)_STRIP)") \
	CFLAGS="$$(_runtime_$(1)_CFLAGS)" \
	CXXFLAGS="$$(_runtime_$(1)_CXXFLAGS)" \
	CPPFLAGS="$$(_runtime_$(1)_CPPFLAGS)" \
	CXXCPPFLAGS="$$(_runtime_$(1)_CXXCPPFLAGS)" \
	LDFLAGS="$$(_runtime_$(1)_LDFLAGS)" \
	$$(_$(1)_CONFIGURE_ENVIRONMENT) \
	$$($(1)_CONFIGURE_ENVIRONMENT)

_runtime_$(1)_CONFIGURE_FLAGS= \
	$$(if $(2),--host=$(2)) \
	--cache-file=$$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION).config.cache \
	--prefix=$$(TOP)/sdks/out/$(1)-$$(CONFIGURATION) \
	$$(if $$(ENABLE_CXX),--enable-cxx) \
	$$(_cross-runtime_$(1)_CONFIGURE_FLAGS) \
	$$(_$(1)_CONFIGURE_FLAGS) \
	$$($(1)_CONFIGURE_FLAGS)

.stamp-$(1)-$$(CONFIGURATION)-configure: $$(TOP)/configure .stamp-$(1)-toolchain
	mkdir -p $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)
	$(if $$(_$(1)_PATH),PATH="$$$$PATH:$$(_$(1)_PATH)") ./wrap-configure.sh $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION) $$(abspath $$<) $$(_runtime_$(1)_AC_VARS) $$(_runtime_$(1)_CONFIGURE_ENVIRONMENT) $$(_runtime_$(1)_CONFIGURE_FLAGS)
	touch $$@

.stamp-$(1)-configure: .stamp-$(1)-$$(CONFIGURATION)-configure
	touch $$@

.PHONY: build-custom-$(1)
build-custom-$(1):
	$$(MAKE) -C $(1)-$$(CONFIGURATION)

.PHONY: setup-custom-$(1)
setup-custom-$(1):
	mkdir -p $$(TOP)/sdks/out/$(1)-$$(CONFIGURATION)

.PHONY: package-$(1)
package-$(1):
	$$(MAKE) -C $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)/mono install
	$$(MAKE) -C $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)/support install

.PHONY: clean-$(1)
clean-$(1):
	rm -rf .stamp-$(1)-toolchain .stamp-$(1)-$$(CONFIGURATION)-configure $$(TOP)/sdks/builds/toolchains/$(1) $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION) $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION).config.cache $$(TOP)/sdks/out/$(1)-$$(CONFIGURATION)

TARGETS += $(1)

endef

$(TOP)/tools/offsets-tool/MonoAotOffsetsDumper.exe: $(wildcard $(TOP)/tools/offsets-tool/*.cs)
	$(MAKE) -C $(dir $@) MonoAotOffsetsDumper.exe

##
# Parameters:
#  $(1): target
#  $(2): host triple
#  $(3): target triple
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
#
# Flags:
#  _$(1)_AR
#  _$(1)_AS
#  _$(1)_CC
#  _$(1)_CPP
#  _$(1)_CXX
#  _$(1)_CXXCPP
#  _$(1)_DLLTOOL
#  _$(1)_LD
#  _$(1)_OBJDUMP
#  _$(1)_RANLIB
#  _$(1)_STRIP
#  _$(1)_CFLAGS
#  _$(1)_CXXFLAGS
#  _$(1)_CPPFLAGS
#  _$(1)_LDFLAGS
#  _$(1)_AC_VARS
#  _$(1)_CONFIGURE_FLAGS
#  _$(1)_PATH
#  _$(1)_OFFSETS_DUMPER_ARGS
define CrossRuntimeTemplate

_cross-runtime_$(1)_CONFIGURE_FLAGS= \
	--target=$(3) \
	--with-cross-offsets=$(3).h \
	--with-llvm=$$(TOP)/sdks/out/$(5)

.stamp-$(1)-toolchain:
	touch $$@

.stamp-$(1)-$$(CONFIGURATION)-configure: | $$(if $$(IGNORE_PROVISION_LLVM),,provision-$(5))

$$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)/$(3).h: .stamp-$(1)-$$(CONFIGURATION)-configure $$(TOP)/tools/offsets-tool/MonoAotOffsetsDumper.exe | configure-$(4)
	cd $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION) && \
		MONO_PATH=$$(TOP)/tools/offsets-tool/CppSharp/$$(if $$(filter $$(UNAME),Darwin),osx_32,$$(if $$(filter $$(UNAME),Linux),linux_64,$$(error "Unknown UNAME='$$(UNAME)'"))) \
			mono $$(if $$(filter $$(UNAME),Darwin),--arch=32) --debug "$$(TOP)/tools/offsets-tool/MonoAotOffsetsDumper.exe" \
				--abi $(6) --outfile "$$@" --mono "$$(TOP)" --targetdir "$$(TOP)/sdks/builds/$(4)-$$(CONFIGURATION)" \
					$$(_$(1)_OFFSETS_DUMPER_ARGS)

build-$(1): $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)/$(3).h

$$(eval $$(call RuntimeTemplate,$(1),$(2)))

endef
