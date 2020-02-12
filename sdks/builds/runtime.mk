
##
# Parameters:
#  $(1): product
#  $(2): target
#  $(3): host triple
#  $(4): exclude from archive
#  $(5): interpreter (either '-interpreter' or empty)
#
# Flags:
#  _$(1)-$(2)_AR
#  _$(1)-$(2)_AS
#  _$(1)-$(2)_CC
#  _$(1)-$(2)_CPP
#  _$(1)-$(2)_CXX
#  _$(1)-$(2)_CXXCPP
#  _$(1)-$(2)_DLLTOOL
#  _$(1)-$(2)_LD
#  _$(1)-$(2)_CMAKE
#  _$(1)-$(2)_OBJDUMP
#  _$(1)-$(2)_RANLIB
#  _$(1)-$(2)_STRIP
#  _$(1)-$(2)_CFLAGS
#  _$(1)-$(2)_CXXFLAGS
#  _$(1)-$(2)_CPPFLAGS
#  _$(1)-$(2)_LDFLAGS
#  _$(1)-$(2)_AC_VARS
#  _$(1)-$(2)_CONFIGURE_FLAGS
#  _$(1)-$(2)_PATH
define RuntimeTemplate

_runtime_$(1)$(5)-$(2)_BITNESS=$$(if $$(or $$(findstring i686,$(3)),$$(findstring i386,$(3))),-m32,$$(if $$(findstring x86_64,$(3)),-m64))

_runtime_$(1)$(5)-$(2)_CFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)$(5)-$(2)_CFLAGS) $$($(1)$(5)-$(2)_CFLAGS) $$(_runtime_$(1)$(5)-$(2)_BITNESS)
_runtime_$(1)$(5)-$(2)_CXXFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)$(5)-$(2)_CXXFLAGS) $$($(1)$(5)-$(2)_CXXFLAGS) $$(_runtime_$(1)$(5)-$(2)_BITNESS)
_runtime_$(1)$(5)-$(2)_CPPFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)$(5)-$(2)_CPPFLAGS) $$($(1)$(5)-$(2)_CPPFLAGS) $$(_runtime_$(1)$(5)-$(2)_BITNESS)
_runtime_$(1)$(5)-$(2)_CXXCPPFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)$(5)-$(2)_CXXCPPFLAGS) $$($(1)$(5)-$(2)_CXXCPPFLAGS) $$(_runtime_$(1)$(5)-$(2)_BITNESS)
_runtime_$(1)$(5)-$(2)_LDFLAGS=$$(_$(1)$(5)-$(2)_LDFLAGS) $$($(1)$(5)-$(2)_LDFLAGS)

_runtime_$(1)$(5)-$(2)_AC_VARS=$$(_$(1)$(5)-$(2)_AC_VARS) $$($(1)$(5)-$(2)_AC_VARS)

_runtime_$(1)$(5)-$(2)_CONFIGURE_ENVIRONMENT = \
	$(if $$(_$(1)$(5)-$(2)_AR),AR="$$(_$(1)$(5)-$(2)_AR)") \
	$(if $$(_$(1)$(5)-$(2)_AS),AS="$$(_$(1)$(5)-$(2)_AS)") \
	$(if $$(_$(1)$(5)-$(2)_CC),CC="$$(_$(1)$(5)-$(2)_CC)") \
	$(if $$(_$(1)$(5)-$(2)_CPP),CPP="$$(_$(1)$(5)-$(2)_CPP)") \
	$(if $$(_$(1)$(5)-$(2)_CXX),CXX="$$(_$(1)$(5)-$(2)_CXX)") \
	$(if $$(_$(1)$(5)-$(2)_CXXCPP),CXXCPP="$$(_$(1)$(5)-$(2)_CXXCPP)") \
	$(if $$(_$(1)$(5)-$(2)_DLLTOOL),DLLTOOL="$$(_$(1)$(5)-$(2)_DLLTOOL)") \
	$(if $$(_$(1)$(5)-$(2)_LD),LD="$$(_$(1)$(5)-$(2)_LD)") \
	$(if $$(_$(1)$(5)-$(2)_OBJDUMP),OBJDUMP="$$(_$(1)$(5)-$(2)_OBJDUMP)") \
	$(if $$(_$(1)$(5)-$(2)_RANLIB),RANLIB="$$(_$(1)$(5)-$(2)_RANLIB)") \
	$(if $$(_$(1)$(5)-$(2)_CMAKE),CMAKE="$$(_$(1)$(5)-$(2)_CMAKE)") \
	$(if $$(_$(1)$(5)-$(2)_STRIP),STRIP="$$(_$(1)$(5)-$(2)_STRIP)") \
	CFLAGS="$$(_runtime_$(1)$(5)-$(2)_CFLAGS)" \
	CXXFLAGS="$$(_runtime_$(1)$(5)-$(2)_CXXFLAGS)" \
	CPPFLAGS="$$(_runtime_$(1)$(5)-$(2)_CPPFLAGS)" \
	CXXCPPFLAGS="$$(_runtime_$(1)$(5)-$(2)_CXXCPPFLAGS)" \
	LDFLAGS="$$(_runtime_$(1)$(5)-$(2)_LDFLAGS)" \
	$$(_$(1)$(5)-$(2)_CONFIGURE_ENVIRONMENT) \
	$$($(1)$(5)-$(2)_CONFIGURE_ENVIRONMENT)

_runtime_$(1)$(5)-$(2)_CONFIGURE_FLAGS= \
	$$(if $(3),--host=$(3)) \
	--cache-file=$$(TOP)/sdks/builds/$(1)$(5)-$(2)-$$(CONFIGURATION).config.cache \
	--prefix=$$(TOP)/sdks/out/$(1)$(5)-$(2)-$$(CONFIGURATION) \
	$$(if $$(ENABLE_CXX),--enable-cxx) \
	$$(_cross-runtime_$(1)$(5)-$(2)_CONFIGURE_FLAGS) \
	$$(_$(1)$(5)-$(2)_CONFIGURE_FLAGS) \
	$$($(1)$(5)-$(2)_CONFIGURE_FLAGS)

.stamp-$(1)$(5)-$(2)-$$(CONFIGURATION)-configure: $$(TOP)/configure .stamp-$(1)$(5)-$(2)-toolchain
	mkdir -p $$(TOP)/sdks/builds/$(1)$(5)-$(2)-$$(CONFIGURATION)
	$(if $$(_$(1)$(5)-$(2)_PATH),PATH="$$$$PATH:$$(_$(1)$(5)-$(2)_PATH)") ./wrap-configure.sh $$(TOP)/sdks/builds/$(1)$(5)-$(2)-$$(CONFIGURATION) $$(abspath $$<) $$(_runtime_$(1)$(5)-$(2)_AC_VARS) $$(_runtime_$(1)$(5)-$(2)_CONFIGURE_ENVIRONMENT) $$(_runtime_$(1)$(5)-$(2)_CONFIGURE_FLAGS)
	touch $$@

.stamp-$(1)$(5)-$(2)-configure: .stamp-$(1)$(5)-$(2)-$$(CONFIGURATION)-configure
	touch $$@

.PHONY: build-custom-$(1)$(5)-$(2)
build-custom-$(1)$(5)-$(2):
	$$(MAKE) -C $(1)$(5)-$(2)-$$(CONFIGURATION)

.PHONY: setup-custom-$(1)$(5)-$(2)
setup-custom-$(1)$(5)-$(2):
	mkdir -p $$(TOP)/sdks/out/$(1)$(5)-$(2)-$$(CONFIGURATION)

.PHONY: package-$(1)$(5)-$(2)
package-$(1)$(5)-$(2):
	$$(MAKE) -C $$(TOP)/sdks/builds/$(1)$(5)-$(2)-$$(CONFIGURATION)/mono install
	$$(MAKE) -C $$(TOP)/sdks/builds/$(1)$(5)-$(2)-$$(CONFIGURATION)/support install

.PHONY: clean-$(1)$(5)-$(2)
clean-$(1)$(5)-$(2):
	rm -rf .stamp-$(1)$(5)-$(2)-toolchain .stamp-$(1)$(5)-$(2)-$$(CONFIGURATION)-configure $$(TOP)/sdks/builds/toolchains/$(1)$(5)-$(2) $$(TOP)/sdks/builds/$(1)$(5)-$(2)-$$(CONFIGURATION) $$(TOP)/sdks/builds/$(1)$(5)-$(2)-$$(CONFIGURATION).config.cache $$(TOP)/sdks/out/$(1)$(5)-$(2)-$$(CONFIGURATION)

$$(eval $$(call TargetTemplate,$(1)$(5),$(2)))

.PHONY: configure-$(1)
configure-$(1): configure-$(1)$(5)-$(2)

.PHONY: build-$(1)
build-$(1): build-$(1)$(5)-$(2)

.PHONY: package-$(1)
package-$(1): package-$(1)$(5)-$(2) $$(ADDITIONAL_PACKAGE_DEPS)

.PHONY: archive-$(1)
archive-$(1): package-$(1)

ifneq ($(4),yes)
$(1)_ARCHIVE += $(1)$(5)-$(2)-$$(CONFIGURATION)
endif

endef

##
# Parameters:
#  $(1): product
#  $(2): target
#  $(3): host triple
#
# Flags:
#  _$(1)-$(2)_AR
#  _$(1)-$(2)_AS
#  _$(1)-$(2)_CC
#  _$(1)-$(2)_CPP
#  _$(1)-$(2)_CXX
#  _$(1)-$(2)_CXXCPP
#  _$(1)-$(2)_DLLTOOL
#  _$(1)-$(2)_LD
#  _$(1)-$(2)_CMAKE
#  _$(1)-$(2)_OBJDUMP
#  _$(1)-$(2)_RANLIB
#  _$(1)-$(2)_STRIP
#  _$(1)-$(2)_CFLAGS
#  _$(1)-$(2)_CXXFLAGS
#  _$(1)-$(2)_CPPFLAGS
#  _$(1)-$(2)_LDFLAGS
#  _$(1)-$(2)_AC_VARS
#  _$(1)-$(2)_CONFIGURE_FLAGS
#  _$(1)-$(2)_PATH
define RuntimeTemplateStub

.stamp-$(1)-$(2)-$$(CONFIGURATION)-configure: $$(TOP)/configure .stamp-$(1)-$(2)-toolchain
	touch $$@

.stamp-$(1)-$(2)-configure: .stamp-$(1)-$(2)-$$(CONFIGURATION)-configure
	touch $$@

.PHONY: build-custom-$(1)-$(2)
build-custom-$(1)-$(2):
	@echo "TODO: build-custom-$(1)-$(2) on $$(UNAME)"

.PHONY: setup-custom-$(1)-$(2)
setup-custom-$(1)-$(2):
	@echo "TODO: setup-custom-$(1)-$(2) on $$(UNAME)"

.PHONY: package-$(1)-$(2)
package-$(1)-$(2):
	@echo "TODO: package-$(1)-$(2) on $$(UNAME)"

.PHONY: clean-$(1)-$(2)
clean-$(1)-$(2):
	rm -rf .stamp-$(1)-$(2)-toolchain .stamp-$(1)-$(2)-$$(CONFIGURATION)-configure $$(TOP)/sdks/builds/toolchains/$(1)-$(2) $$(TOP)/sdks/builds/$(1)-$(2)-$$(CONFIGURATION) $$(TOP)/sdks/builds/$(1)-$(2)-$$(CONFIGURATION).config.cache $$(TOP)/sdks/out/$(1)-$(2)-$$(CONFIGURATION)

$$(eval $$(call TargetTemplate,$(1),$(2)))

.PHONY: configure-$(1)
configure-$(1): configure-$(1)-$(2)

.PHONY: build-$(1)
build-$(1): build-$(1)-$(2)

.PHONY: archive-$(1)
archive-$(1): package-$(1)-$(2)


endef


##
# Parameters:
#  $(1): product
#  $(2): target
#  $(3): host triple
#  $(4): target triple
#  $(5): device target
#  $(6): llvm
#  $(7): offsets dumper abi
#
# Flags:
#  _$(1)-$(2)_AR
#  _$(1)-$(2)_AS
#  _$(1)-$(2)_CC
#  _$(1)-$(2)_CPP
#  _$(1)-$(2)_CXX
#  _$(1)-$(2)_CXXCPP
#  _$(1)-$(2)_DLLTOOL
#  _$(1)-$(2)_LD
#  _$(1)-$(2)_OBJDUMP
#  _$(1)-$(2)_RANLIB
#  _$(1)-$(2)_STRIP
#  _$(1)-$(2)_CFLAGS
#  _$(1)-$(2)_CXXFLAGS
#  _$(1)-$(2)_CPPFLAGS
#  _$(1)-$(2)_LDFLAGS
#  _$(1)-$(2)_AC_VARS
#  _$(1)-$(2)_CONFIGURE_FLAGS
#  _$(1)-$(2)_PATH
#  _$(1)-$(2)_OFFSETS_DUMPER_ARGS
define CrossRuntimeTemplate

_cross-runtime_$(1)-$(2)_CONFIGURE_FLAGS= \
	--target=$(4) \
	--with-cross-offsets=$(4).h \
	--with-llvm=$$(TOP)/sdks/out/$(6)

.stamp-$(1)-$(2)-toolchain:
	touch $$@

.stamp-$(1)-$(2)-$$(CONFIGURATION)-configure: | $$(if $$(IGNORE_PROVISION_LLVM),,provision-$(6))

$$(TOP)/sdks/builds/$(1)-$(2)-$$(CONFIGURATION)/$(4).h: .stamp-$(1)-$(2)-$$(CONFIGURATION)-configure | configure-$(1)-$(5)
	python3 $(TOP)/mono/tools/offsets-tool/offsets-tool.py --targetdir="$$(TOP)/sdks/builds/$(1)-$(5)-$$(CONFIGURATION)" --abi=$(7) --monodir="$$(TOP)" --outfile="$$@" $$(_$(1)-$(2)_OFFSETS_DUMPER_ARGS)

build-$(1)-$(2): $$(TOP)/sdks/builds/$(1)-$(2)-$$(CONFIGURATION)/$(4).h

$$(eval $$(call RuntimeTemplate,$(1),$(2),$(3)))

.PHONY: archive-$(1)
archive-$(1): provision-$(6)

$(1)_ARCHIVE += $(6)

endef

##
# Parameters:
#  $(1): product
#  $(2): target
#  $(3): host triple
#  $(4): target triple
#  $(5): device target
#  $(6): llvm
#  $(7): offsets dumper abi
#
# Flags:
#  _$(1)-$(2)_AR
#  _$(1)-$(2)_AS
#  _$(1)-$(2)_CC
#  _$(1)-$(2)_CPP
#  _$(1)-$(2)_CXX
#  _$(1)-$(2)_CXXCPP
#  _$(1)-$(2)_DLLTOOL
#  _$(1)-$(2)_LD
#  _$(1)-$(2)_OBJDUMP
#  _$(1)-$(2)_RANLIB
#  _$(1)-$(2)_STRIP
#  _$(1)-$(2)_CFLAGS
#  _$(1)-$(2)_CXXFLAGS
#  _$(1)-$(2)_CPPFLAGS
#  _$(1)-$(2)_LDFLAGS
#  _$(1)-$(2)_AC_VARS
#  _$(1)-$(2)_CONFIGURE_FLAGS
#  _$(1)-$(2)_PATH
#  _$(1)-$(2)_OFFSETS_DUMPER_ARGS
define CrossRuntimeTemplateStub

.stamp-$(1)-$(2)-toolchain:
	touch $$@

.stamp-$(1)-$(2)-$$(CONFIGURATION)-configure:

$$(eval $$(call RuntimeTemplateStub,$(1),$(2),$(3)))

endef

