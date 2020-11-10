
maccat_PKG_CONFIG_DIR = $(TOP)/sdks/out/maccat-pkgconfig
maccat_LIBS_DIR = $(TOP)/sdks/out/maccat-libs
maccat_TPN_DIR = $(TOP)/sdks/out/maccat-tpn
maccat_MONO_VERSION = $(TOP)/sdks/out/maccat-mono-version.txt

maccat_ARCHIVE += maccat-pkgconfig maccat-libs maccat-tpn maccat-mono-version.txt
ADDITIONAL_PACKAGE_DEPS += $(maccat_PKG_CONFIG_DIR) $(maccat_LIBS_DIR) $(maccat_TPN_DIR) $(maccat_MONO_VERSION)

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): xcode dir
define MacCatTemplate

maccat_$(1)_PLATFORM_BIN=$(3)/Toolchains/XcodeDefault.xctoolchain/usr/bin

#
# HACK: fak: The -target is placed in the CC define per the recommendation of
# libtool who acknowledge that some parameters are just not passed through
# to the compiler. You can use -Wc, flags, but I failed to get the working appropriately.
#
_maccat-$(1)_CC=$$(CCACHE) $$(maccat_$(1)_PLATFORM_BIN)/clang -target $(2)-apple-ios$(MACCAT_IOS_VERSION_MIN)-macabi
_maccat-$(1)_CXX=$$(CCACHE) $$(maccat_$(1)_PLATFORM_BIN)/clang++ -target $(2)-apple-ios$(MACCAT_IOS_VERSION_MIN)-macabi

_maccat-$(1)_AC_VARS= \
	ac_cv_func_system=no \
	ac_cv_c_bigendian=no \
	ac_cv_func_fstatat=no \
	ac_cv_func_readlinkat=no \
	ac_cv_func_getpwuid_r=no \
	ac_cv_func_posix_getpwuid_r=yes \
	ac_cv_header_curses_h=no \
	ac_cv_header_localcharset_h=no \
	ac_cv_header_sys_user_h=no \
	ac_cv_func_getentropy=no \
	ac_cv_func_futimens=no \
	ac_cv_func_utimensat=no \
	ac_cv_func_shm_open_working_with_mmap=no \
	mono_cv_sizeof_sunpath=104

_maccat-$(1)_CFLAGS= \
	$$(maccat-$(1)_SYSROOT) \
	-fexceptions

_maccat-$(1)_CXXFLAGS= \
	$$(maccat-$(1)_SYSROOT)

_maccat-$(1)_CPPFLAGS= \
	-DSMALL_CONFIG -D_XOPEN_SOURCE -DHOST_IOS -DHOST_MACCAT -DHAVE_LARGE_FILE_SUPPORT=1

_maccat-$(1)_LDFLAGS= \
	-iframework $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk/System/iOSSupport/System/Library/Frameworks \
	-framework CoreFoundation \
	-lobjc -lc++

_maccat-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-executables \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--disable-visibility-hidden \
	--enable-dtrace=no \
	--enable-maintainer-mode \
	--enable-minimal=ssa,com,interpreter,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging,remoting,shared_perfcounters,gac \
	--enable-monotouch \
	--with-lazy-gc-thread-creation=yes \
	--with-tls=pthread \
	--without-ikvm-native \
	--without-sigaltstack \
	--disable-cooperative-suspend \
	--disable-hybrid-suspend \
	--disable-crash-reporting

.stamp-maccat-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,maccat,$(1),$(2)-apple-darwin10,yes))

endef

maccat-mac64_SYSROOT=-isysroot $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk

$(eval $(call MacCatTemplate,mac64,x86_64,$(XCODE_DIR)))

$(eval $(call BclTemplate,maccat,monotouch,monotouch))

$(maccat_PKG_CONFIG_DIR): package-maccat-mac64
	rm -rf $(maccat_PKG_CONFIG_DIR)
	mkdir -p $(maccat_PKG_CONFIG_DIR)

	cp $(TOP)/sdks/builds/maccat-mac64-$(CONFIGURATION)/data/mono-2.pc $(maccat_PKG_CONFIG_DIR)

$(maccat_LIBS_DIR): package-maccat-mac64
	rm -rf $(maccat_LIBS_DIR)
	mkdir -p $(maccat_LIBS_DIR)

	cp $(TOP)/sdks/out/maccat-mac64-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib        $(maccat_LIBS_DIR)/libmonosgen-2.0.dylib
	cp $(TOP)/sdks/out/maccat-mac64-$(CONFIGURATION)/lib/libmono-native.dylib         $(maccat_LIBS_DIR)/libmono-native.dylib
	cp $(TOP)/sdks/out/maccat-mac64-$(CONFIGURATION)/lib/libMonoPosixHelper.dylib     $(maccat_LIBS_DIR)/libMonoPosixHelper.dylib
	cp $(TOP)/sdks/out/maccat-mac64-$(CONFIGURATION)/lib/libmonosgen-2.0.a            $(maccat_LIBS_DIR)/libmonosgen-2.0.a
	cp $(TOP)/sdks/out/maccat-mac64-$(CONFIGURATION)/lib/libmono-native.a             $(maccat_LIBS_DIR)/libmono-native.a
	cp $(TOP)/sdks/out/maccat-mac64-$(CONFIGURATION)/lib/libmono-profiler-log.a       $(maccat_LIBS_DIR)/libmono-profiler-log.a

	$(maccat_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(maccat_LIBS_DIR)/libmonosgen-2.0.dylib
	$(maccat_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native.dylib         $(maccat_LIBS_DIR)/libmono-native.dylib
	$(maccat_mac64_PLATFORM_BIN)/install_name_tool -id @rpath/libMonoPosixHelper.dylib     $(maccat_LIBS_DIR)/libMonoPosixHelper.dylib

$(maccat_MONO_VERSION): $(TOP)/configure.ac
	mkdir -p $(dir $(maccat_MONO_VERSION))
	grep AC_INIT $(TOP)/configure.ac | sed -e 's/.*\[//' -e 's/\].*//' > $@

$(maccat_TPN_DIR)/LICENSE:
	mkdir -p $(maccat_TPN_DIR)
	cd $(TOP) && rsync -r --include='THIRD-PARTY-NOTICES.TXT' --include='license.txt' --include='License.txt' --include='LICENSE' --include='LICENSE.txt' --include='LICENSE.TXT' --include='COPYRIGHT.regex' --include='*/' --exclude="*" --prune-empty-dirs . $(maccat_TPN_DIR)

$(maccat_TPN_DIR): $(maccat_TPN_DIR)/LICENSE
