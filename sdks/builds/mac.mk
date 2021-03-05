
mac_BIN_DIR = $(TOP)/sdks/out/mac-bin
mac_PKG_CONFIG_DIR = $(TOP)/sdks/out/mac-pkgconfig
mac_LIBS_DIR = $(TOP)/sdks/out/mac-libs
mac_TPN_DIR = $(TOP)/sdks/out/mac-tpn
mac_MONO_VERSION = $(TOP)/sdks/out/mac-mono-version.txt

mac_ARCHIVE += mac-bin mac-pkgconfig mac-libs mac-tpn mac-mono-version.txt
ADDITIONAL_PACKAGE_DEPS += $(mac_BIN_DIR) $(mac_PKG_CONFIG_DIR) $(mac_LIBS_DIR) $(mac_TPN_DIR) $(mac_MONO_VERSION)

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): host arch for compiler (x86_64 or arm64)
#  $(4): xcode dir
define MacTemplate

mac_$(1)_PLATFORM_BIN=$(4)/Toolchains/XcodeDefault.xctoolchain/usr/bin

_mac-$(1)_CC=$$(CCACHE) $$(mac_$(1)_PLATFORM_BIN)/clang
_mac-$(1)_CXX=$$(CCACHE) $$(mac_$(1)_PLATFORM_BIN)/clang++

_mac-$(1)_AC_VARS= \
	ac_cv_func_fstatat=no \
	ac_cv_func_readlinkat=no \
	ac_cv_func_futimens=no \
	ac_cv_func_utimensat=no

_mac-$(1)_CFLAGS= \
	$$(mac-$(1)_SYSROOT) \
	-arch $(3)

_mac-$(1)_CXXFLAGS= \
	$$(mac-$(1)_SYSROOT) \
	-arch $(3)

_mac-$(1)_CPPFLAGS=

_mac-$(1)_LDFLAGS=

# Xcode 12 and later cause issues with no_weak_imports: https://github.com/mono/mono/issues/19393
ifeq ($(XCODE_MAJOR_VERSION),$(filter $(XCODE_MAJOR_VERSION), 11 10 9))
_mac-$(1)_LDFLAGS += -Wl,-no_weak_imports
endif

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

##
# Cross compiler builds
#
# Parameters:
#  $(1): target (crossarm64)
#  $(2): host arch (x86_64 or aarch64)
#  $(3): target arch (arm or aarch64)
#  $(4): device target (target64, targetarm64 ...)
#  $(5): llvm
#  $(6): offsets dumper abi
#  $(7): sysroot path
#
# Flags:
#  mac-$(1)_AC_VARS
#  mac-$(1)_CFLAGS
#  mac-$(1)_CXXFLAGS
#  mac-$(1)_LDFLAGS
#  mac-$(1)_CONFIGURE_FLAGS
define MacCrossTemplate

_mac-$(1)_OFFSETS_DUMPER_ARGS=--libclang="$$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/lib/libclang.dylib" --sysroot="$(7)"
_mac_$(1)_PLATFORM_BIN=$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/bin

_mac-$(1)_CC=$$(CCACHE) $$(_mac_$(1)_PLATFORM_BIN)/clang
_mac-$(1)_CXX=$$(CCACHE) $$(_mac_$(1)_PLATFORM_BIN)/clang++

_mac-$(1)_AC_VARS= \
	ac_cv_func_fstatat=no \
	ac_cv_func_readlinkat=no \
	ac_cv_func_futimens=no \
	ac_cv_func_utimensat=no

_mac-$(1)_CFLAGS= \
	$$(mac-$(1)_SYSROOT) \
	-arch $(2) \
	-Qunused-arguments

_mac-$(1)_CXXFLAGS= \
	$$(mac-$(1)_SYSROOT) \
	-arch $(2) \
	-Qunused-arguments \
	-stdlib=libc++

_mac-$(1)_CPPFLAGS= \
	-arch $(2) \

_mac-$(1)_LDFLAGS= \
	-stdlib=libc++

_mac-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--enable-dtrace=no \
	--enable-maintainer-mode \
	--with-glib=embedded \
	--with-mcs-docs=no

$$(eval $$(call CrossRuntimeTemplate,mac,$(1),$(2)-apple-darwin20,$(3),$(4),$(5),$(6)))

endef

mac_sysroot_path = $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk
mac_sysroot = -isysroot $(mac_sysroot_path)

mac-mac64_SYSROOT=$(mac_sysroot) -mmacosx-version-min=$(MACOS_VERSION_MIN)
mac-macarm64_SYSROOT=$(mac_sysroot) -mmacosx-version-min=$(MACOS_VERSION_MIN)

mac-crossarm64_SYSROOT=$(mac_sysroot) -mmacosx-version-min=$(MACOS_VERSION_MIN)

$(eval $(call MacTemplate,mac64,x86_64,x86_64,$(XCODE_DIR)))
$(eval $(call MacTemplate,macarm64,aarch64,arm64,$(XCODE_DIR)))

$(eval $(call MacCrossTemplate,crossarm64,x86_64,aarch64-apple-darwin20.0.0,macarm64,llvm-llvm64,aarch64-apple-darwin20,$(mac_sysroot_path)))

$(eval $(call BclTemplate,mac,xammac xammac_net_4_5,xammac xammac_net_4_5))

$(mac_BIN_DIR): package-mac-mac64 package-mac-macarm64 package-mac-crossarm64
	rm -rf $(mac_BIN_DIR)
	mkdir -p $(mac_BIN_DIR)

	cp $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/bin/mono-sgen $(mac_BIN_DIR)/mono-sgen
	cp $(TOP)/sdks/out/mac-crossarm64-$(CONFIGURATION)/bin/aarch64-apple-darwin20.0.0-mono-sgen $(mac_BIN_DIR)/aarch64-darwin-mono-sgen

$(mac_PKG_CONFIG_DIR): package-mac-mac64 package-mac-macarm64
	rm -rf $(mac_PKG_CONFIG_DIR)
	mkdir -p $(mac_PKG_CONFIG_DIR)

	cp $(TOP)/sdks/builds/mac-mac64-$(CONFIGURATION)/data/mono-2.pc $(mac_PKG_CONFIG_DIR)

$(mac_LIBS_DIR): package-mac-mac64 package-mac-macarm64
	rm -rf $(mac_LIBS_DIR)
	mkdir -p $(mac_LIBS_DIR)

	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib        $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib        -create -output $(mac_LIBS_DIR)/libmonosgen-2.0.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-compat.dylib  $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libmono-native-compat.dylib  -create -output $(mac_LIBS_DIR)/libmono-native-compat.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-unified.dylib $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libmono-native-unified.dylib -create -output $(mac_LIBS_DIR)/libmono-native-unified.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libMonoPosixHelper.dylib     $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libMonoPosixHelper.dylib     -create -output $(mac_LIBS_DIR)/libMonoPosixHelper.dylib
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmonosgen-2.0.a            $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libmonosgen-2.0.a            -create -output $(mac_LIBS_DIR)/libmonosgen-2.0.a
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-compat.a      $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libmono-native-compat.a      -create -output $(mac_LIBS_DIR)/libmono-native-compat.a
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-native-unified.a     $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libmono-native-unified.a     -create -output $(mac_LIBS_DIR)/libmono-native-unified.a
	$(mac_mac64_PLATFORM_BIN)/lipo $(TOP)/sdks/out/mac-mac64-$(CONFIGURATION)/lib/libmono-profiler-log.a       $(TOP)/sdks/out/mac-macarm64-$(CONFIGURATION)/lib/libmono-profiler-log.a       -create -output $(mac_LIBS_DIR)/libmono-profiler-log.a

	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(mac_LIBS_DIR)/libmonosgen-2.0.dylib
	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  $(mac_LIBS_DIR)/libmono-native-compat.dylib
	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib $(mac_LIBS_DIR)/libmono-native-unified.dylib
	$(mac_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libMonoPosixHelper.dylib     $(mac_LIBS_DIR)/libMonoPosixHelper.dylib

$(mac_MONO_VERSION): $(TOP)/configure.ac
	mkdir -p $(dir $(mac_MONO_VERSION))
	grep AC_INIT $(TOP)/configure.ac | sed -e 's/.*\[//' -e 's/\].*//' > $@

$(mac_TPN_DIR)/LICENSE:
	mkdir -p $(mac_TPN_DIR)
	cd $(TOP) && rsync -r --include='THIRD-PARTY-NOTICES.TXT' --include='license.txt' --include='License.txt' --include='LICENSE' --include='LICENSE.txt' --include='LICENSE.TXT' --include='COPYRIGHT.regex' --include='*/' --exclude="*" --prune-empty-dirs . $(mac_TPN_DIR)

$(mac_TPN_DIR): $(mac_TPN_DIR)/LICENSE
