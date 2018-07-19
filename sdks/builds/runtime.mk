
##
# Parameters:
#  $(1): target
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

__$(1)_CFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CFLAGS)
__$(1)_CXXFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CXXFLAGS)
__$(1)_CPPFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CPPFLAGS)
__$(1)_CXXCPPFLAGS=$(if $(RELEASE),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) $$(_$(1)_CXXCPPFLAGS)

__$(1)_CONFIGURE_ENVIRONMENT = \
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
	CFLAGS="$$(__$(1)_CFLAGS)" \
	CXXFLAGS="$$(__$(1)_CXXFLAGS)" \
	CPPFLAGS="$$(__$(1)_CPPFLAGS)" \
	CXXCPPFLAGS="$$(__$(1)_CXXCPPFLAGS)" \
	$(if $$(_$(1)_LDFLAGS),LDFLAGS="$$(_$(1)_LDFLAGS)") \
	$$(_$(1)_CONFIGURE_ENVIRONMENT)

.stamp-$(1)-$$(CONFIGURATION)-configure: $$(TOP)/configure .stamp-$(1)-toolchain
	mkdir -p $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)
	$(if $$(_$(1)_PATH),PATH="$$$$PATH:$$(_$(1)_PATH)") ./wrap-configure.sh $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION) $(abspath $(TOP)/configure) $$(_$(1)_AC_VARS) $$(__$(1)_CONFIGURE_ENVIRONMENT) $$(_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: .stamp-$(1)-configure
.stamp-$(1)-configure: .stamp-$(1)-$$(CONFIGURATION)-configure

.PHONY: build-custom-$(1)
build-custom-$(1):
	$$(MAKE) -C $(1)-$$(CONFIGURATION)

.PHONY: setup-custom-$(1)
setup-custom-$(1):
	mkdir -p $$(TOP)/sdks/out/$(1)-$$(CONFIGURATION)

.PHONY: package-$(1)-$$(CONFIGURATION)
package-$(1)-$$(CONFIGURATION):
	$$(MAKE) -C $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)/mono install
	$$(MAKE) -C $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION)/support install

.PHONY: package-$(1)
package-$(1):
	$$(MAKE) package-$(1)-$$(CONFIGURATION)

.PHONY: clean-$(1)-$$(CONFIGURATION)
clean-$(1)-$$(CONFIGURATION):
	rm -rf .stamp-$(1)-toolchain .stamp-$(1)-$$(CONFIGURATION)-configure $$(TOP)/sdks/builds/toolchains/$(1) $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION) $$(TOP)/sdks/builds/$(1)-$$(CONFIGURATION).config.cache $$(TOP)/sdks/out/$(1)-$$(CONFIGURATION)

.PHONY: clean-$(1)
clean-$(1):
	$$(MAKE) clean-$(1)-$$(CONFIGURATION)

TARGETS += $(1)

endef
