
mac_PKG_CONFIG_DIR = $(TOP)/sdks/out/mac-pkgconfig
mac_LIBS_DIR = $(TOP)/sdks/out/mac-libs
mac_ARCHIVE += mac-pkgconfig mac-libs

ADDITIONAL_PACKAGE_DEPS += $(mac_PKG_CONFIG_DIR) $(mac_LIBS_DIR)

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): xcode dir
define MacTemplate

mac_$(1)_PLATFORM_BIN=$(3)/Toolchains/XcodeDefault.xctoolchain/usr/bin

_mac-$(1)_CC=$$(CCACHE) $$(mac_$(1)_PLATFORM_BIN)/clang
_mac-$(1)_CXX=$$(CCACHE) $$(mac_$(1)_PLATFORM_BIN)/clang++

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
	-isysroot $(3)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk \
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
	--with-mcs-docs=no

.stamp-mac-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,mac,$(1),$(2)-apple-darwin10,yes))

endef

$(eval $(call MacTemplate,mac32,i386,$(XCODE32_DIR)))
$(eval $(call MacTemplate,mac64,x86_64,$(XCODE_DIR)))

$(eval $(call BclTemplate,mac,xammac xammac_net_4_5,xammac xammac_net_4_5))

$(mac_PKG_CONFIG_DIR): package-mac-mac64
	rm -rf $(mac_PKG_CONFIG_DIR)
	mkdir -p $(mac_PKG_CONFIG_DIR)

	cp $(TOP)/sdks/builds/mac-mac64-$(CONFIGURATION)/data/mono-2.pc $(mac_PKG_CONFIG_DIR)

$(mac_LIBS_DIR): package-mac-mac32 package-mac-mac64
	rm -rf $(mac_LIBS_DIR)
	mkdir -p $(mac_LIBS_DIR)

	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib        $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib        -create -output $(mac_LIBS_DIR)/libmonosgen-2.0.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libmono-native-compat.dylib  $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-compat.dylib  -create -output $(mac_LIBS_DIR)/libmono-native-compat.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libmono-native-unified.dylib $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-unified.dylib -create -output $(mac_LIBS_DIR)/libmono-native-unified.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libMonoPosixHelper.dylib     $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libMonoPosixHelper.dylib     -create -output $(mac_LIBS_DIR)/libMonoPosixHelper.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libmonosgen-2.0.a            $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmonosgen-2.0.a            -create -output $(mac_LIBS_DIR)/libmonosgen-2.0.a
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libmono-native-compat.a      $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-compat.a      -create -output $(mac_LIBS_DIR)/libmono-native-compat.a
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libmono-native-unified.a     $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-unified.a     -create -output $(mac_LIBS_DIR)/libmono-native-unified.a
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac32-$(CONFIGURATION)/lib/libmono-profiler-log.a       $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-profiler-log.a       -create -output $(mac_LIBS_DIR)/libmono-profiler-log.a
	
	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(mac_LIBS_DIR)/libmonosgen-2.0.dylib
	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  $(mac_LIBS_DIR)/libmono-native-compat.dylib
	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib $(mac_LIBS_DIR)/libmono-native-unified.dylib
	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libMonoPosixHelper.dylib     $(mac_LIBS_DIR)/libMonoPosixHelper.dylib
