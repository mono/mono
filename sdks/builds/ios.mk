
#
# Targets:
# - build-ios-<target>
#    Build <target>
# - package-ios-<target>
#    Install target into ../out/<target>
# - clean-ios-<target>
#    Clean target
# Where <target> is: target32, target32s, target64, sim32, sim64, cross32, cross64
#

ios_FRAMEWORKS_DIR = $(TOP)/sdks/out/ios-frameworks
ios_LIBS_DIR = $(TOP)/sdks/out/ios-libs
ios_SOURCES_DIR = $(TOP)/sdks/out/ios-sources
ios_TPN_DIR = $(TOP)/sdks/out/ios-tpn
ios_MONO_VERSION = $(TOP)/sdks/out/ios-mono-version.txt

ios_ARCHIVE += ios-frameworks ios-libs ios-sources ios-tpn ios-mono-version.txt
ADDITIONAL_PACKAGE_DEPS += $(ios_FRAMEWORKS_DIR) $(ios_LIBS_DIR) $(ios_SOURCES_DIR) $(ios_TPN_DIR) $(ios_MONO_VERSION)

ios_PLATFORM_BIN=$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/bin

##
# Device builds
#
# Parameters
#  $(1): target (target32/target32s/target64)
#  $(2): host triple
#  $(3): host arch for compiler (armv7 or arm64)
#
# Flags:
#  ios-$(1)_AC_VARS
#  ios-$(1)_SYSROOT
#  ios-$(1)_CONFIGURE_FLAGS
#  ios-$(1)_CFLAGS
#  ios-$(1)_CPPFLAGS
#  ios-$(1)_CXXFLAGS
#  ios-$(1)_LDFLAGS
#  ios-$(1)_BITCODE_MARKER
#
# This handles tvos/watchos as well.
#
define iOSDeviceTemplate

_ios-$(1)_CC=$$(CCACHE) $$(ios_PLATFORM_BIN)/clang
_ios-$(1)_CXX=$$(CCACHE) $$(ios_PLATFORM_BIN)/clang++

_ios-$(1)_AC_VARS= \
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
	mono_cv_sizeof_sunpath=104 \
	mono_cv_uscore=yes

_ios-$(1)_CFLAGS= \
	$$(ios-$(1)_SYSROOT) \
	-arch $(3) \
	-Wl,-application_extension \
	-fexceptions \
	$$(ios-$(1)_BITCODE_MARKER)

_ios-$(1)_CXXFLAGS= \
	$$(ios-$(1)_SYSROOT) \
	-arch $(3) \
	-Wl,-application_extension \
	$$(ios-$(1)_BITCODE_MARKER)

_ios-$(1)_CPPFLAGS= \
	-DMONOTOUCH=1 \
	$$(ios-$(1)_SYSROOT) \
	-arch $(3) \
	-DSMALL_CONFIG -D_XOPEN_SOURCE -DHOST_IOS -DHAVE_LARGE_FILE_SUPPORT=1 \

_ios-$(1)_LDFLAGS= \
	-Wl,-no_weak_imports \
	-arch $(3) \
	-framework CoreFoundation \
	-lobjc -lc++

_ios-$(1)_CONFIGURE_FLAGS = \
	--disable-boehm \
	--disable-btls \
	--disable-executables \
	--disable-icall-tables \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--disable-visibility-hidden \
	--enable-dtrace=no \
	--enable-icall-export \
	--enable-maintainer-mode \
	--enable-minimal=ssa,com,interpreter,jit,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging,remoting,shared_perfcounters,gac \
	--enable-monotouch \
	--with-lazy-gc-thread-creation=yes \
	--with-tls=pthread \
	--without-ikvm-native \
	--without-sigaltstack \
	--disable-cooperative-suspend \
	--disable-hybrid-suspend \
	--disable-crash-reporting

.stamp-ios-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,ios,$(1),$(2),yes))

## Create special versions of the .dylibs:
#
# We have the following requirements:
#
# * libmonosgen-2.0.dylib: must have miphone-version-min=7.0 (otherwise iOS 9 won't load it; see bug #34267).
# * libmono-profiler-log.dylib: same as libmonosgen-2.0.dylib
# * libmono-native-compat.dylib: same as libmonosgen-2.0.dylib
# * Mono.framework/Mono: must have miphone-version-min=8.0, otherwise the native linker won't add a LC_ENCRYPTION_INFO load command,
#   which the App Store requires (see bug #32820). This is not a problem for libmonosgen-2.0.dylib, because that library is only
#   used for incremental builds, which are not published).
#
# So what we do is to take the static library (libmonosgen-2.0.a), extract all the object files, and re-link
# them the required times according to how many versions we need.

$$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmonosgen-2.0-minversion70.dylib: package-ios-$(1)
	CC="$$(_ios-$(1)_CC)" $$(TOP)/sdks/builds/create-shared-library.sh $$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmonosgen-2.0.a $$@ -arch $(3) -miphoneos-version-min=7.0 $$(ios_sysroot)

$$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmono-profiler-log-minversion70.dylib: package-ios-$(1)
	CC="$$(_ios-$(1)_CC)" $$(TOP)/sdks/builds/create-shared-library.sh $$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmono-profiler-log-static.a $$@ -arch $(3) -miphoneos-version-min=7.0 $$(ios_sysroot) -L$$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib -lmonosgen-2.0

$$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmono-native-compat-minversion70.dylib: package-ios-$(1)
	CC="$$(_ios-$(1)_CC)" $$(TOP)/sdks/builds/create-shared-library.sh $$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmono-native-compat.a $$@ -arch $(3) -miphoneos-version-min=7.0 $$(ios_sysroot) -L$$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib -lmonosgen-2.0 -framework GSS

$$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmonosgen-2.0-minversion80.dylib: package-ios-$(1)
	CC="$$(_ios-$(1)_CC)" $$(TOP)/sdks/builds/create-shared-library.sh $$(TOP)/sdks/out/ios-$(1)-$$(CONFIGURATION)/lib/libmonosgen-2.0.a $$@ -arch $(3) -miphoneos-version-min=8.0 $$(ios_sysroot)

endef

ios_sysroot_path = $(XCODE_DIR)/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS$(IOS_VERSION).sdk
tvos_sysroot_path = $(XCODE_DIR)/Platforms/AppleTVOS.platform/Developer/SDKs/AppleTVOS$(TVOS_VERSION).sdk
watchos_sysroot_path = $(XCODE_DIR)/Platforms/WatchOS.platform/Developer/SDKs/WatchOS$(WATCHOS_VERSION).sdk
watchos64_32_sysroot_path = $(XCODE_DIR)/Platforms/WatchOS.platform/Developer/SDKs/WatchOS$(WATCHOS64_32_VERSION).sdk

ios_sysroot = -isysroot $(ios_sysroot_path)
tvos_sysroot = -isysroot $(tvos_sysroot_path)
watchos_sysroot = -isysroot $(watchos_sysroot_path)
watchos64_32_sysroot = -isysroot $(watchos64_32_sysroot_path)

# explicitly disable dtrace, since it requires inline assembly, which is disabled on AppleTV (and mono's configure.ac doesn't know that (yet at least))
ios-targettv_CONFIGURE_FLAGS = 	--enable-dtrace=no --enable-llvm-runtime --with-bitcode=yes
ios-targetwatch_CONFIGURE_FLAGS = --enable-cooperative-suspend --enable-llvm-runtime --with-bitcode=yes
ios-targetwatch64_32_CONFIGURE_FLAGS = --enable-cooperative-suspend --enable-llvm-runtime --with-bitcode=yes

ios-target32_SYSROOT = $(ios_sysroot) -miphoneos-version-min=$(IOS_VERSION_MIN)
ios-target32s_SYSROOT = $(ios_sysroot) -miphoneos-version-min=$(IOS_VERSION_MIN)
ios-target64_SYSROOT = $(ios_sysroot) -miphoneos-version-min=$(IOS_VERSION_MIN)
ios-targettv_SYSROOT = $(tvos_sysroot) -mtvos-version-min=$(TVOS_VERSION_MIN)
ios-targetwatch_SYSROOT = $(watchos_sysroot) -mwatchos-version-min=$(WATCHOS_VERSION_MIN)
ios-targetwatch64_32_SYSROOT = $(watchos64_32_sysroot) -mwatchos-version-min=$(WATCHOS64_32_VERSION_MIN)

ios-target32_CPPFLAGS = -DHOST_IOS
ios-target32s_CPPFLAGS = -DHOST_IOS
ios-target64_CPPFLAGS = -DHOST_IOS
ios-targettv_CPPFLAGS = -DHOST_IOS -DHOST_TVOS
ios-targetwatch_CPPFLAGS = -DHOST_IOS -DHOST_WATCHOS
ios-targetwatch64_32_CPPFLAGS = -DHOST_IOS -DHOST_WATCHOS

ios-targettv_CFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targettv_CXXFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targetwatch_CFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targetwatch_CXXFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targetwatch64_32_CFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targetwatch64_32_CXXFLAGS = -fembed-bitcode -fno-gnu-inline-asm

ios-targettv_LDFLAGS = -Wl,-bitcode_bundle -framework CoreFoundation -lobjc -lc++
ios-targetwatch_LDFLAGS = -Wl,-bitcode_bundle -framework CoreFoundation -lobjc -lc++
ios-targetwatch64_32_LDFLAGS = -Wl,-bitcode_bundle -framework CoreFoundation -lobjc -lc++

ios-targettv_AC_VARS = \
	ac_cv_func_system=no			\
	ac_cv_func_pthread_kill=no      \
	ac_cv_func_kill=no              \
	ac_cv_func_sigaction=no         \
	ac_cv_func_fork=no              \
	ac_cv_func_execv=no             \
	ac_cv_func_execve=no            \
	ac_cv_func_execvp=no            \
	ac_cv_func_signal=no
ios-targetwatch_AC_VARS = $(ios-targettv_AC_VARS)
ios-targetwatch64_32_AC_VARS = $(ios-targettv_AC_VARS)

# ios-target32_BITCODE_MARKER=-fembed-bitcode-marker
$(eval $(call iOSDeviceTemplate,target32,arm-apple-darwin10,armv7))
$(eval $(call iOSDeviceTemplate,target32s,arm-apple-darwin10,armv7s))
# ios-target64_BITCODE_MARKER=-fembed-bitcode-marker
$(eval $(call iOSDeviceTemplate,target64,aarch64-apple-darwin10,arm64))
$(eval $(call iOSDeviceTemplate,targettv,aarch64-apple-darwin10,arm64))
$(eval $(call iOSDeviceTemplate,targetwatch,armv7k-apple-darwin10,armv7k))
$(eval $(call iOSDeviceTemplate,targetwatch64_32,aarch64-apple-darwin10_ilp32,arm64_32))

##
# Simulator builds
#
# Parameters
#  $(1): target (sim32 or sim64)
#  $(2): host triple
#  $(3): host arch (i386 or x86_64)
#
# Flags:
#  ios-$(1)_SYSROOT
#  ios-$(1)_AC_VARS
#  ios-$(1)_CFLAGS
#  ios-$(1)_CPPFLAGS
#  ios-$(1)_CXXFLAGS
#  ios-$(1)_LDFLAGS
#
# This handles tvos/watchos as well.
#
define iOSSimulatorTemplate

_ios-$(1)_CC=$$(CCACHE) $$(ios_PLATFORM_BIN)/clang
_ios-$(1)_CXX=$$(CCACHE) $$(ios_PLATFORM_BIN)/clang++

_ios-$(1)_AC_VARS= \
	ac_cv_func_clock_nanosleep=no \
	ac_cv_func_fstatat=no \
	ac_cv_func_readlinkat=no \
	ac_cv_func_system=no \
	ac_cv_func_getentropy=no \
	ac_cv_func_futimens=no \
	ac_cv_func_utimensat=no \
	ac_cv_func_shm_open_working_with_mmap=no \
	mono_cv_uscore=yes

_ios-$(1)_CFLAGS= \
	$$(ios-$(1)_SYSROOT) \
	-arch $(3) \
	-Wl,-application_extension

_ios-$(1)_CPPFLAGS= \
	-DMONOTOUCH=1 \
	$$(ios-$(1)_SYSROOT) \
	-arch $(3) \
	-Wl,-application_extension

_ios-$(1)_CXXFLAGS= \
	$$(ios-$(1)_SYSROOT) \
	-arch $(3) \
	-Wl,-application_extension

_ios-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-executables \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--disable-visibility-hidden \
	--enable-maintainer-mode \
	--enable-minimal=com,remoting,shared_perfcounters,gac \
	--enable-monotouch \
	--with-tls=pthread \
	--without-ikvm-native \
	--disable-cooperative-suspend \
	--disable-hybrid-suspend \
	--disable-crash-reporting

.stamp-ios-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,ios,$(1),$(2),yes))

endef

ios_sim_sysroot = -isysroot $(XCODE_DIR)/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator$(IOS_VERSION).sdk 
tvos_sim_sysroot = -isysroot $(XCODE_DIR)/Platforms/AppleTVSimulator.platform/Developer/SDKs/AppleTVSimulator$(TVOS_VERSION).sdk
watchos_sim_sysroot = -isysroot $(XCODE_DIR)/Platforms/WatchSimulator.platform/Developer/SDKs/WatchSimulator$(WATCHOS_VERSION).sdk

ios-sim32_SYSROOT = $(ios_sim_sysroot) -mios-simulator-version-min=$(IOS_VERSION_MIN)
ios-sim64_SYSROOT = $(ios_sim_sysroot) -mios-simulator-version-min=$(IOS_VERSION_MIN)
ios-simtv_SYSROOT = $(tvos_sim_sysroot) -mtvos-simulator-version-min=$(TVOS_VERSION_MIN)
ios-simwatch_SYSROOT = $(watchos_sim_sysroot) -mwatchos-simulator-version-min=$(WATCHOS_VERSION_MIN)

ios-simwatch_CONFIGURE_FLAGS = --enable-cooperative-suspend

ios-sim32_CPPFLAGS = -DHOST_IOS
ios-sim64_CPPFLAGS = -DHOST_IOS
ios-simtv_CPPFLAGS = -DHOST_IOS -DHOST_TVOS
ios-simwatch_CPPFLAGS = -DHOST_IOS -DHOST_WATCHOS

ios-simtv_AC_VARS = \
	ac_cv_func_pthread_kill=no \
	ac_cv_func_kill=no \
	ac_cv_func_sigaction=no \
	ac_cv_func_fork=no \
	ac_cv_func_execv=no \
	ac_cv_func_execve=no \
	ac_cv_func_execvp=no \
	ac_cv_func_signal=no
ios-simwatch_AC_VARS =  \
	ac_cv_func_system=no \
	ac_cv_func_pthread_kill=no \
	ac_cv_func_kill=no \
	ac_cv_func_sigaction=no \
	ac_cv_func_fork=no \
	ac_cv_func_execv=no \
	ac_cv_func_execve=no \
	ac_cv_func_execvp=no \
	ac_cv_func_signal=no

$(eval $(call iOSSimulatorTemplate,sim32,i386-apple-darwin10,i386))
$(eval $(call iOSSimulatorTemplate,sim64,x86_64-apple-darwin10,x86_64))
$(eval $(call iOSSimulatorTemplate,simtv,x86_64-apple-darwin10,x86_64))
$(eval $(call iOSSimulatorTemplate,simwatch,i386-apple-darwin10,i386))

##
# Cross compiler builds
#
# Parameters:
#  $(1): target (cross32 or cross64)
#  $(2): host arch (i386 or x86_64)
#  $(3): target arch (arm or aarch64)
#  $(4): device target (target32, target64, ...)
#  $(5): llvm
#  $(6): offsets dumper abi
#  $(7): sysroot path
#
# Flags:
#  ios-$(1)_AC_VARS
#  ios-$(1)_CFLAGS
#  ios-$(1)_CXXFLAGS
#  ios-$(1)_LDFLAGS
#  ios-$(1)_CONFIGURE_FLAGS
define iOSCrossTemplate

_ios-$(1)_OFFSETS_DUMPER_ARGS=--libclang="$$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/lib/libclang.dylib" --sysroot="$(7)"
_ios_$(1)_PLATFORM_BIN=$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/bin

_ios-$(1)_CC=$$(CCACHE) $$(_ios_$(1)_PLATFORM_BIN)/clang
_ios-$(1)_CXX=$$(CCACHE) $$(_ios_$(1)_PLATFORM_BIN)/clang++

_ios-$(1)_AC_VARS= \
	ac_cv_func_shm_open_working_with_mmap=no

_ios-$(1)_CFLAGS= \
	$$(ios-$(1)_SYSROOT) \
	-Qunused-arguments

_ios-$(1)_CXXFLAGS= \
	$$(ios-$(1)_SYSROOT) \
	-Qunused-arguments \
	-stdlib=libc++

_ios-$(1)_CPPFLAGS= \
	-DMONOTOUCH=1

_ios-$(1)_LDFLAGS= \
	-stdlib=libc++

_ios-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-btls \
	--disable-iconv \
	--disable-libraries \
	--disable-mcs-build \
	--disable-nls \
	--enable-dtrace=no \
	--enable-icall-symbol-map \
	--enable-minimal=com,remoting \
	--enable-monotouch \
	--disable-crash-reporting

$$(eval $$(call CrossRuntimeTemplate,ios,$(1),$(2)-apple-darwin10,$(3),$(4),$(5),$(6)))

endef

ios-cross32_SYSROOT=-isysroot $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk -mmacosx-version-min=$(MACOS_VERSION_MIN)
ios-crosswatch_SYSROOT=-isysroot $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk -mmacosx-version-min=$(MACOS_VERSION_MIN)
ios-cross64_SYSROOT=-isysroot $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk -mmacosx-version-min=$(MACOS_VERSION_MIN)
ios-crosswatch64_32_SYSROOT=-isysroot $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk -mmacosx-version-min=$(MACOS_VERSION_MIN)
ios-cross32-64_SYSROOT=-isysroot $(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$(MACOS_VERSION).sdk -mmacosx-version-min=$(MACOS_VERSION_MIN)

ios-crosswatch_CONFIGURE_FLAGS=--enable-cooperative-suspend

$(eval $(call iOSCrossTemplate,cross32,x86_64,arm-darwin,target32,llvm-llvm64,arm-apple-darwin10,$(ios_sysroot_path)))
$(eval $(call iOSCrossTemplate,cross64,x86_64,aarch64-darwin,target64,llvm-llvm64,aarch64-apple-darwin10,$(ios_sysroot_path)))
$(eval $(call iOSCrossTemplate,crosswatch,x86_64,armv7k-unknown-darwin,targetwatch,llvm-llvm64,armv7k-apple-darwin,$(watchos_sysroot_path)))
$(eval $(call iOSCrossTemplate,crosswatch64_32,x86_64,aarch64-apple-darwin10_ilp32,targetwatch64_32,llvm-llvm64,aarch64-apple-darwin10_ilp32,$(watchos64_32_sysroot_path)))


$(ios_FRAMEWORKS_DIR): package-ios-target32 package-ios-target32s package-ios-target64 package-ios-targettv package-ios-targetwatch package-ios-targetwatch64_32 package-ios-sim32 package-ios-sim64 package-ios-simtv package-ios-simwatch $(TOP)/sdks/builds/ios-Mono.framework-Info.plist $(TOP)/sdks/builds/ios-Mono.framework-tvos.Info.plist $(TOP)/sdks/builds/ios-Mono.framework-watchos.Info.plist $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion80.dylib $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion80.dylib $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion80.dylib
	rm -rf $(ios_FRAMEWORKS_DIR)

	### Mono.framework for devices ###
	mkdir -p $(ios_FRAMEWORKS_DIR)/ios/Mono.framework/
	mkdir -p $(ios_FRAMEWORKS_DIR)/tvos/Mono.framework/
	mkdir -p $(ios_FRAMEWORKS_DIR)/watchos/Mono.framework/
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion80.dylib $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion80.dylib $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion80.dylib -create -output $(ios_FRAMEWORKS_DIR)/ios/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib -create -output $(ios_FRAMEWORKS_DIR)/tvos/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib -create -output $(ios_FRAMEWORKS_DIR)/watchos/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/Mono.framework/Mono $(ios_FRAMEWORKS_DIR)/ios/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/Mono.framework/Mono $(ios_FRAMEWORKS_DIR)/tvos/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/Mono.framework/Mono $(ios_FRAMEWORKS_DIR)/watchos/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_FRAMEWORKS_DIR)/ios/Mono.framework.dSYM $(ios_FRAMEWORKS_DIR)/ios/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_FRAMEWORKS_DIR)/tvos/Mono.framework.dSYM $(ios_FRAMEWORKS_DIR)/tvos/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_FRAMEWORKS_DIR)/watchos/Mono.framework.dSYM $(ios_FRAMEWORKS_DIR)/watchos/Mono.framework/Mono
	cp $(TOP)/sdks/builds/ios-Mono.framework-Info.plist $(ios_FRAMEWORKS_DIR)/ios/Mono.framework/Info.plist
	cp $(TOP)/sdks/builds/ios-Mono.framework-tvos.Info.plist $(ios_FRAMEWORKS_DIR)/tvos/Mono.framework/Info.plist
	cp $(TOP)/sdks/builds/ios-Mono.framework-watchos.Info.plist $(ios_FRAMEWORKS_DIR)/watchos/Mono.framework/Info.plist

	### Mono.framework for simulators ###
	mkdir -p $(ios_FRAMEWORKS_DIR)/ios-sim/Mono.framework/
	mkdir -p $(ios_FRAMEWORKS_DIR)/tvos-sim/Mono.framework/
	mkdir -p $(ios_FRAMEWORKS_DIR)/watchos-sim/Mono.framework/
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib -create -output $(ios_FRAMEWORKS_DIR)/ios-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib -create -output $(ios_FRAMEWORKS_DIR)/tvos-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib -create -output $(ios_FRAMEWORKS_DIR)/watchos-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/Mono.framework/Mono $(ios_FRAMEWORKS_DIR)/ios-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/Mono.framework/Mono $(ios_FRAMEWORKS_DIR)/tvos-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/Mono.framework/Mono $(ios_FRAMEWORKS_DIR)/watchos-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_FRAMEWORKS_DIR)/ios-sim/Mono.framework.dSYM $(ios_FRAMEWORKS_DIR)/ios-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_FRAMEWORKS_DIR)/tvos-sim/Mono.framework.dSYM $(ios_FRAMEWORKS_DIR)/tvos-sim/Mono.framework/Mono
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_FRAMEWORKS_DIR)/watchos-sim/Mono.framework.dSYM $(ios_FRAMEWORKS_DIR)/watchos-sim/Mono.framework/Mono
	cp $(TOP)/sdks/builds/ios-Mono.framework-Info.plist $(ios_FRAMEWORKS_DIR)/ios-sim/Mono.framework/Info.plist
	cp $(TOP)/sdks/builds/ios-Mono.framework-tvos.Info.plist $(ios_FRAMEWORKS_DIR)/tvos-sim/Mono.framework/Info.plist
	cp $(TOP)/sdks/builds/ios-Mono.framework-watchos.Info.plist $(ios_FRAMEWORKS_DIR)/watchos-sim/Mono.framework/Info.plist


$(ios_LIBS_DIR): package-ios-target32 package-ios-target32s package-ios-target64 package-ios-targettv package-ios-targetwatch package-ios-targetwatch64_32 package-ios-sim32 package-ios-sim64 package-ios-simtv package-ios-simwatch $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion70.dylib $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion70.dylib $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion70.dylib $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-profiler-log-minversion70.dylib $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-profiler-log-minversion70.dylib $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-profiler-log-minversion70.dylib $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-native-compat-minversion70.dylib $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-native-compat-minversion70.dylib $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-native-compat-minversion70.dylib
	rm -rf $(ios_LIBS_DIR)

	### libs for devices ###
	mkdir -p $(ios_LIBS_DIR)/ios/
	mkdir -p $(ios_LIBS_DIR)/tvos/
	mkdir -p $(ios_LIBS_DIR)/watchos/

	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion70.dylib       $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion70.dylib       $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0-minversion70.dylib       -create -output $(ios_LIBS_DIR)/ios/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-profiler-log-minversion70.dylib  $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-profiler-log-minversion70.dylib  $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-profiler-log-minversion70.dylib  -create -output $(ios_LIBS_DIR)/ios/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-native-compat-minversion70.dylib $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-native-compat-minversion70.dylib $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-native-compat-minversion70.dylib -create -output $(ios_LIBS_DIR)/ios/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-native-unified.dylib             $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-native-unified.dylib             $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-native-unified.dylib             -create -output $(ios_LIBS_DIR)/ios/libmono-native-unified.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-ee-interp.a                      $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-ee-interp.a                      $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-ee-interp.a                      -create -output $(ios_LIBS_DIR)/ios/libmono-ee-interp.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-icall-table.a                    $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-icall-table.a                    $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-icall-table.a                    -create -output $(ios_LIBS_DIR)/ios/libmono-icall-table.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-ilgen.a                          $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-ilgen.a                          $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-ilgen.a                          -create -output $(ios_LIBS_DIR)/ios/libmono-ilgen.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-native-compat.a                  $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-native-compat.a                  $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-native-compat.a                  -create -output $(ios_LIBS_DIR)/ios/libmono-native-compat.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-native-unified.a                 $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-native-unified.a                 $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-native-unified.a                 -create -output $(ios_LIBS_DIR)/ios/libmono-native-unified.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmono-profiler-log-static.a            $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmono-profiler-log-static.a            $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmono-profiler-log-static.a            -create -output $(ios_LIBS_DIR)/ios/libmono-profiler-log.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0.a                        $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0.a                        $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0.a                        -create -output $(ios_LIBS_DIR)/ios/libmonosgen-2.0.a

	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib          -create -output $(ios_LIBS_DIR)/tvos/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-profiler-log.dylib     -create -output $(ios_LIBS_DIR)/tvos/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-native-compat.dylib    -create -output $(ios_LIBS_DIR)/tvos/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-native-unified.dylib   -create -output $(ios_LIBS_DIR)/tvos/libmono-native-unified.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-ee-interp.a            -create -output $(ios_LIBS_DIR)/tvos/libmono-ee-interp.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-icall-table.a          -create -output $(ios_LIBS_DIR)/tvos/libmono-icall-table.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-ilgen.a                -create -output $(ios_LIBS_DIR)/tvos/libmono-ilgen.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-native-compat.a        -create -output $(ios_LIBS_DIR)/tvos/libmono-native-compat.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-native-unified.a       -create -output $(ios_LIBS_DIR)/tvos/libmono-native-unified.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmono-profiler-log-static.a  -create -output $(ios_LIBS_DIR)/tvos/libmono-profiler-log.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmonosgen-2.0.a              -create -output $(ios_LIBS_DIR)/tvos/libmonosgen-2.0.a

	$(ios_PLATFORM_BIN)/bitcode_strip $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib               -m -o $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0-stripped.dylib
	$(ios_PLATFORM_BIN)/bitcode_strip $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-profiler-log.dylib          -m -o $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-profiler-log-stripped.dylib
	$(ios_PLATFORM_BIN)/bitcode_strip $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-compat.dylib         -m -o $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-compat-stripped.dylib
	$(ios_PLATFORM_BIN)/bitcode_strip $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-unified.dylib        -m -o $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-unified-stripped.dylib
	$(ios_PLATFORM_BIN)/bitcode_strip $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib          -m -o $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmonosgen-2.0-stripped.dylib
	$(ios_PLATFORM_BIN)/bitcode_strip $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-profiler-log.dylib     -m -o $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-profiler-log-stripped.dylib
	$(ios_PLATFORM_BIN)/bitcode_strip $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-native.dylib           -m -o $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-native-stripped.dylib

	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0-stripped.dylib         $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmonosgen-2.0-stripped.dylib        -create -output $(ios_LIBS_DIR)/watchos/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-profiler-log-stripped.dylib    $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-profiler-log-stripped.dylib   -create -output $(ios_LIBS_DIR)/watchos/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-compat-stripped.dylib                                                                                                   -create -output $(ios_LIBS_DIR)/watchos/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-unified-stripped.dylib  $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-native-stripped.dylib         -create -output $(ios_LIBS_DIR)/watchos/libmono-native-unified.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-ee-interp.a                    $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-ee-interp.a                   -create -output $(ios_LIBS_DIR)/watchos/libmono-ee-interp.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-icall-table.a                  $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-icall-table.a                 -create -output $(ios_LIBS_DIR)/watchos/libmono-icall-table.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-ilgen.a                        $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-ilgen.a                       -create -output $(ios_LIBS_DIR)/watchos/libmono-ilgen.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-compat.a                                                                                                                -create -output $(ios_LIBS_DIR)/watchos/libmono-native-compat.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-native-unified.a               $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-native.a                      -create -output $(ios_LIBS_DIR)/watchos/libmono-native-unified.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmono-profiler-log-static.a          $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmono-profiler-log-static.a         -create -output $(ios_LIBS_DIR)/watchos/libmono-profiler-log.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.a                      $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmonosgen-2.0.a                     -create -output $(ios_LIBS_DIR)/watchos/libmonosgen-2.0.a

	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(ios_LIBS_DIR)/ios/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-profiler-log.dylib   -change $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/ios/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  -change $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/ios/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib -change $(TOP)/sdks/out/ios-target32-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-target32s-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-target64-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/ios/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(ios_LIBS_DIR)/tvos/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-profiler-log.dylib   -change $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/tvos/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  -change $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/tvos/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib -change $(TOP)/sdks/out/ios-targettv-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/tvos/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(ios_LIBS_DIR)/watchos/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-profiler-log.dylib   -change $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/watchos/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  -change $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib                                                                                                                        $(ios_LIBS_DIR)/watchos/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib -change $(TOP)/sdks/out/ios-targetwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib -change $(TOP)/sdks/out/ios-targetwatch64_32-$(CONFIGURATION)/lib/libmonosgen-2.0.1.dylib @rpath/libmonosgen-2.0.dylib $(ios_LIBS_DIR)/watchos/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios/libmonosgen-2.0.dylib.dSYM        $(ios_LIBS_DIR)/ios/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios/libmono-profiler-log.dylib.dSYM   $(ios_LIBS_DIR)/ios/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios/libmono-native-compat.dylib.dSYM  $(ios_LIBS_DIR)/ios/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios/libmono-native-unified.dylib.dSYM $(ios_LIBS_DIR)/ios/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos/libmonosgen-2.0.dylib.dSYM        $(ios_LIBS_DIR)/tvos/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos/libmono-profiler-log.dylib.dSYM   $(ios_LIBS_DIR)/tvos/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos/libmono-native-compat.dylib.dSYM  $(ios_LIBS_DIR)/tvos/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos/libmono-native-unified.dylib.dSYM $(ios_LIBS_DIR)/tvos/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos/libmonosgen-2.0.dylib.dSYM        $(ios_LIBS_DIR)/watchos/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos/libmono-profiler-log.dylib.dSYM   $(ios_LIBS_DIR)/watchos/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos/libmono-native-compat.dylib.dSYM  $(ios_LIBS_DIR)/watchos/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos/libmono-native-unified.dylib.dSYM $(ios_LIBS_DIR)/watchos/libmono-native-unified.dylib

	### libs for simulators ###
	mkdir -p $(ios_LIBS_DIR)/ios-sim/
	mkdir -p $(ios_LIBS_DIR)/tvos-sim/
	mkdir -p $(ios_LIBS_DIR)/watchos-sim/

	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib          $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib         -create -output $(ios_LIBS_DIR)/ios-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmono-profiler-log.dylib     $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmono-profiler-log.dylib    -create -output $(ios_LIBS_DIR)/ios-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmono-native-compat.dylib    $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmono-native-compat.dylib   -create -output $(ios_LIBS_DIR)/ios-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmono-native-unified.dylib   $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmono-native-unified.dylib  -create -output $(ios_LIBS_DIR)/ios-sim/libmono-native-unified.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmono-native-compat.a        $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmono-native-compat.a       -create -output $(ios_LIBS_DIR)/ios-sim/libmono-native-compat.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmono-native-unified.a       $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmono-native-unified.a      -create -output $(ios_LIBS_DIR)/ios-sim/libmono-native-unified.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmono-profiler-log-static.a  $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmono-profiler-log-static.a -create -output $(ios_LIBS_DIR)/ios-sim/libmono-profiler-log.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-sim32-$(CONFIGURATION)/lib/libmonosgen-2.0.a              $(TOP)/sdks/out/ios-sim64-$(CONFIGURATION)/lib/libmonosgen-2.0.a             -create -output $(ios_LIBS_DIR)/ios-sim/libmonosgen-2.0.a

	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib          -create -output $(ios_LIBS_DIR)/tvos-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmono-profiler-log.dylib     -create -output $(ios_LIBS_DIR)/tvos-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmono-native-compat.dylib    -create -output $(ios_LIBS_DIR)/tvos-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmono-native-unified.dylib   -create -output $(ios_LIBS_DIR)/tvos-sim/libmono-native-unified.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmono-native-compat.a        -create -output $(ios_LIBS_DIR)/tvos-sim/libmono-native-compat.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmono-native-unified.a       -create -output $(ios_LIBS_DIR)/tvos-sim/libmono-native-unified.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmono-profiler-log-static.a  -create -output $(ios_LIBS_DIR)/tvos-sim/libmono-profiler-log.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simtv-$(CONFIGURATION)/lib/libmonosgen-2.0.a              -create -output $(ios_LIBS_DIR)/tvos-sim/libmonosgen-2.0.a

	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.dylib          -create -output $(ios_LIBS_DIR)/watchos-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmono-profiler-log.dylib     -create -output $(ios_LIBS_DIR)/watchos-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmono-native-compat.dylib    -create -output $(ios_LIBS_DIR)/watchos-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmono-native-unified.dylib   -create -output $(ios_LIBS_DIR)/watchos-sim/libmono-native-unified.dylib
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmono-native-compat.a        -create -output $(ios_LIBS_DIR)/watchos-sim/libmono-native-compat.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmono-native-unified.a       -create -output $(ios_LIBS_DIR)/watchos-sim/libmono-native-unified.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmono-profiler-log-static.a  -create -output $(ios_LIBS_DIR)/watchos-sim/libmono-profiler-log.a
	$(ios_PLATFORM_BIN)/lipo $(TOP)/sdks/out/ios-simwatch-$(CONFIGURATION)/lib/libmonosgen-2.0.a              -create -output $(ios_LIBS_DIR)/watchos-sim/libmonosgen-2.0.a

	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(ios_LIBS_DIR)/ios-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-profiler-log.dylib   $(ios_LIBS_DIR)/ios-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  $(ios_LIBS_DIR)/ios-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib $(ios_LIBS_DIR)/ios-sim/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(ios_LIBS_DIR)/tvos-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-profiler-log.dylib   $(ios_LIBS_DIR)/tvos-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  $(ios_LIBS_DIR)/tvos-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib $(ios_LIBS_DIR)/tvos-sim/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmonosgen-2.0.dylib        $(ios_LIBS_DIR)/watchos-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-profiler-log.dylib   $(ios_LIBS_DIR)/watchos-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-compat.dylib  $(ios_LIBS_DIR)/watchos-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/install_name_tool -id @rpath/libmono-native-unified.dylib $(ios_LIBS_DIR)/watchos-sim/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios-sim/libmonosgen-2.0.dylib.dSYM        $(ios_LIBS_DIR)/ios-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios-sim/libmono-profiler-log.dylib.dSYM   $(ios_LIBS_DIR)/ios-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios-sim/libmono-native-compat.dylib.dSYM  $(ios_LIBS_DIR)/ios-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/ios-sim/libmono-native-unified.dylib.dSYM $(ios_LIBS_DIR)/ios-sim/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos-sim/libmonosgen-2.0.dylib.dSYM        $(ios_LIBS_DIR)/tvos-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos-sim/libmono-profiler-log.dylib.dSYM   $(ios_LIBS_DIR)/tvos-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos-sim/libmono-native-compat.dylib.dSYM  $(ios_LIBS_DIR)/tvos-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/tvos-sim/libmono-native-unified.dylib.dSYM $(ios_LIBS_DIR)/tvos-sim/libmono-native-unified.dylib

	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos-sim/libmonosgen-2.0.dylib.dSYM        $(ios_LIBS_DIR)/watchos-sim/libmonosgen-2.0.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos-sim/libmono-profiler-log.dylib.dSYM   $(ios_LIBS_DIR)/watchos-sim/libmono-profiler-log.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos-sim/libmono-native-compat.dylib.dSYM  $(ios_LIBS_DIR)/watchos-sim/libmono-native-compat.dylib
	$(ios_PLATFORM_BIN)/dsymutil -t 4 -o $(ios_LIBS_DIR)/watchos-sim/libmono-native-unified.dylib.dSYM $(ios_LIBS_DIR)/watchos-sim/libmono-native-unified.dylib

$(ios_SOURCES_DIR)/mcs/build/common/Consts.cs:  # we use this as a sentinel file to avoid rsyncing everything on each build (slows down iterating)
	mkdir -p $(ios_SOURCES_DIR)
	cd $(TOP) && rsync -r --exclude='external/api-doc-tools/*' --exclude='external/api-snapshot/*' --exclude='external/aspnetwebstack/*' --exclude='external/binary-reference-assemblies/*' --exclude='netcore/*' --include='*.cs' --include='*/' --exclude="*" --prune-empty-dirs . $(ios_SOURCES_DIR)

$(ios_SOURCES_DIR): $(ios_SOURCES_DIR)/mcs/build/common/Consts.cs

$(ios_TPN_DIR)/LICENSE:
	mkdir -p $(ios_TPN_DIR)
	cd $(TOP) && rsync -r --include='THIRD-PARTY-NOTICES.TXT' --include='license.txt' --include='License.txt' --include='LICENSE' --include='LICENSE.txt' --include='LICENSE.TXT' --include='COPYRIGHT.regex' --include='*/' --exclude="*" --prune-empty-dirs . $(ios_TPN_DIR)

$(ios_TPN_DIR): $(ios_TPN_DIR)/LICENSE

$(ios_MONO_VERSION): $(TOP)/configure.ac
	mkdir -p $(dir $(ios_MONO_VERSION))
	grep AC_INIT $(TOP)/configure.ac | sed -e 's/.*\[//' -e 's/\].*//' > $@

##
# BCL builds
##
$(eval $(call BclTemplate,ios,monotouch monotouch_runtime monotouch_tv monotouch_tv_runtime monotouch_watch monotouch_watch_runtime monotouch_tools,monotouch monotouch_tv monotouch_watch))
