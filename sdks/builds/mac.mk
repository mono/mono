
mac_PLATFORM_BIN=$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/bin

##
# Parameters
#  $(1): target
define MacTemplate

_mac-$(1)_CC=$$(CCACHE) $$(mac_PLATFORM_BIN)/clang
_mac-$(1)_CXX=$$(CCACHE) $$(mac_PLATFORM_BIN)/clang++

_mac-$(1)_AC_VARS= \
	ac_cv_func_fstatat=no \
	ac_cv_func_readlinkat=no \
	ac_cv_func_futimens=no \
	ac_cv_func_utimensat=no

_mac-$(1)_CFLAGS= \
	-arch $(2)

_mac-$(1)_CXXFLAGS= \
	-arch $(2)

_mac-$(1)_CPPFLAGS= \
	-isysroot $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk \
	-mmacosx-version-min=$(MACOS_VERSION_MIN) \

_mac-$(1)_LDFLAGS= \
	-Wl,-no_weak_imports

_mac-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--enable-maintainer-mode \
	--with-glib=embedded \
	--with-mcs-docs=no \
	$$(if $$(wildcard $$(TOP)/../mono-extensions),--enable-extension-module=xamarin --enable-extension-module)

.stamp-mac-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,mac-$(1),$(2)-apple-darwin10))

mac_TARGETS += mac-$(1)-$$(CONFIGURATION)

endef

$(eval $(call MacTemplate,mac32,i386))
$(eval $(call MacTemplate,mac64,x86_64))

$(eval $(call BclTemplate,mac-bcl,xammac xammac_net_4_5,xammac xammac_net_4_5))
mac_TARGETS += mac-bcl
