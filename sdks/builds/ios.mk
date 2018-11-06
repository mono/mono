
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

PLATFORM_BIN=$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/bin

##
# Device builds
#
# Parameters
#  $(1): target (target32/target32s/target64)
#  $(2): host arch (arm or aarch64)
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

_ios-$(1)_CC=$$(CCACHE) $$(PLATFORM_BIN)/clang
_ios-$(1)_CXX=$$(CCACHE) $$(PLATFORM_BIN)/clang++

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
	--enable-minimal=ssa,com,interpreter,jit,reflection_emit_save,reflection_emit,portability,assembly_remapping,attach,verifier,full_messages,appdomains,security,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging,remoting,shared_perfcounters \
	--enable-monotouch \
	--with-lazy-gc-thread-creation=yes \
	--with-monotouch \
	--with-tls=pthread \
	--without-ikvm-native \
	--without-sigaltstack \
	--disable-cooperative-suspend \
	--disable-hybrid-suspend \
	--disable-crash-reporting

.stamp-ios-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,ios-$(1),$(2)-apple-darwin10))

ios_TARGETS += ios-$(1)-$$(CONFIGURATION)

endef

ios_sysroot = -isysroot $(XCODE_DIR)/Platforms/iPhoneOS.platform/Developer/SDKs/iPhoneOS$(IOS_VERSION).sdk -miphoneos-version-min=$(IOS_VERSION_MIN)
tvos_sysroot = -isysroot $(XCODE_DIR)/Platforms/AppleTVOS.platform/Developer/SDKs/AppleTVOS$(TVOS_VERSION).sdk 	-mtvos-version-min=$(TVOS_VERSION_MIN)
watchos_sysroot = -isysroot $(XCODE_DIR)/Platforms/WatchOS.platform/Developer/SDKs/WatchOS$(WATCH_VERSION).sdk -mwatchos-version-min=$(WATCHOS_VERSION_MIN)

# explicitly disable dtrace, since it requires inline assembly, which is disabled on AppleTV (and mono's configure.ac doesn't know that (yet at least))
ios-targettv_CONFIGURE_FLAGS = 	--enable-dtrace=no --enable-llvm-runtime --with-bitcode=yes --with-monotouch-tv
ios-targetwatch_CONFIGURE_FLAGS = --enable-cooperative-suspend --enable-llvm-runtime --with-bitcode=yes --with-monotouch-watch

ios-target32_SYSROOT = $(ios_sysroot)
ios-target32s_SYSROOT = $(ios_sysroot)
ios-target64_SYSROOT = $(ios_sysroot)
ios-targettv_SYSROOT = $(tvos_sysroot)
ios-targetwatch_SYSROOT = $(watchos_sysroot)

ios-target32_CPPFLAGS = -DHOST_IOS
ios-target32s_CPPFLAGS = -DHOST_IOS
ios-target64_CPPFLAGS = -DHOST_IOS
ios-targettv_CPPFLAGS = -DHOST_APPLETVOS -DTARGET_APPLETVOS
ios-targetwatch_CPPFLAGS = -DHOST_IOS -DHOST_WATCHOS

ios-targettv_CFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targettv_CXXFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targetwatch_CFLAGS = -fembed-bitcode -fno-gnu-inline-asm
ios-targetwatch_CXXFLAGS = -fembed-bitcode -fno-gnu-inline-asm

ios-targettv_LDFLAGS = -Wl,-bitcode_bundle -framework CoreFoundation -lobjc -lc++
ios-targetwatch_LDFLAGS = -Wl,-bitcode_bundle -framework CoreFoundation -lobjc -lc++

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

# ios-target32_BITCODE_MARKER=-fembed-bitcode-marker
$(eval $(call iOSDeviceTemplate,target32,arm,armv7))
$(eval $(call iOSDeviceTemplate,target32s,arm,armv7s))
# ios-target64_BITCODE_MARKER=-fembed-bitcode-marker
$(eval $(call iOSDeviceTemplate,target64,aarch64,arm64))
$(eval $(call iOSDeviceTemplate,targettv,aarch64,arm64))
$(eval $(call iOSDeviceTemplate,targetwatch,armv7k,armv7k))

##
# Simulator builds
#
# Parameters
#  $(1): target (sim32 or sim64)
#  $(2): host arch (i386 or x86_64)
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

_ios-$(1)_CC=$$(CCACHE) $$(PLATFORM_BIN)/clang
_ios-$(1)_CXX=$$(CCACHE) $$(PLATFORM_BIN)/clang++

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
	-arch $(2) \
	-Wl,-application_extension

_ios-$(1)_CPPFLAGS= \
	-DMONOTOUCH=1 \
	$$(ios-$(1)_SYSROOT) \
	-arch $(2) \
	-Wl,-application_extension

_ios-$(1)_CXXFLAGS= \
	$$(ios-$(1)_SYSROOT) \
	-arch $(2) \
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
	--enable-minimal=com,remoting,shared_perfcounters \
	--enable-monotouch \
	--with-tls=pthread \
	--without-ikvm-native \
	--disable-cooperative-suspend \
	--disable-hybrid-suspend \
	--disable-crash-reporting

# _ios-$(1)_CONFIGURE_FLAGS += --enable-extension-module=xamarin

.stamp-ios-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,ios-$(1),$(2)-apple-darwin10))

ios_TARGETS += ios-$(1)-$$(CONFIGURATION)

endef

ios_sim_sysroot = -isysroot $(XCODE_DIR)/Platforms/iPhoneSimulator.platform/Developer/SDKs/iPhoneSimulator$(IOS_VERSION).sdk -mios-simulator-version-min=$(IOS_VERSION_MIN)
tvos_sim_sysroot = -isysroot $(XCODE_DIR)/Platforms/AppleTVSimulator.platform/Developer/SDKs/AppleTVSimulator$(TVOS_VERSION).sdk -mtvos-simulator-version-min=$(TVOS_VERSION_MIN)
watchos_sim_sysroot = -isysroot $(XCODE_DIR)/Platforms/WatchSimulator.platform/Developer/SDKs/WatchSimulator$(WATCH_VERSION).sdk -mwatchos-simulator-version-min=$(WATCHOS_VERSION_MIN)

ios-sim32_SYSROOT = $(ios_sim_sysroot)
ios-sim64_SYSROOT = $(ios_sim_sysroot)
ios-simtv_SYSROOT = $(tvos_sim_sysroot)
ios-simwatch_SYSROOT = $(watchos_sim_sysroot)

ios-simwatch_CONFIGURE_FLAGS = --enable-cooperative-suspend

ios-sim32_CPPFLAGS = -DHOST_IOS
ios-sim64_CPPFLAGS = -DHOST_IOS
ios-simtv_CPPFLAGS = -DHOST_APPLETVOS -DTARGET_APPLETVOS
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

$(eval $(call iOSSimulatorTemplate,sim32,i386))
$(eval $(call iOSSimulatorTemplate,sim64,x86_64))
$(eval $(call iOSSimulatorTemplate,simtv,x86_64))
$(eval $(call iOSSimulatorTemplate,simwatch,i386))

##
# Parameters:
#  $(1): target (cross32 or cross64)
#  $(2): host arch (i386 or x86_64)
#  $(3): target arch (arm or aarch64)
#  $(4): device target (target32, target64, ...)
#  $(5): llvm (llvm32 or llvm64)
#  $(6): offsets dumper abi
#
# Flags:
#  ios-$(1)_AC_VARS
#  ios-$(1)_CFLAGS
#  ios-$(1)_CXXFLAGS
#  ios-$(1)_LDFLAGS
#  ios-$(1)_CONFIGURE_FLAGS
define iOSCrossTemplate

_ios-$(1)_OFFSETS_DUMPER_ARGS=--gen-ios

_ios-$(1)_CC=$$(CCACHE) $$(PLATFORM_BIN)/clang
_ios-$(1)_CXX=$$(CCACHE) $$(PLATFORM_BIN)/clang++

_ios-$(1)_AC_VARS= \
	ac_cv_func_shm_open_working_with_mmap=no

_ios-$(1)_CFLAGS= \
	-isysroot $$(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$$(MACOS_VERSION).sdk -mmacosx-version-min=$$(MACOS_VERSION_MIN) \
	-Qunused-arguments

_ios-$(1)_CXXFLAGS= \
	-isysroot $$(XCODE_DIR)/Platforms/MacOSX.platform/Developer/SDKs/MacOSX$$(MACOS_VERSION).sdk -mmacosx-version-min=$$(MACOS_VERSION_MIN) \
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
	--disable-crash-reporting

$$(eval $$(call CrossRuntimeTemplate,ios-$(1),$(2)-apple-darwin10,$(3)-darwin,$(4),$(5),$(6)))

ios_TARGETS += ios-$(1)-$$(CONFIGURATION) $(5)

endef

$(eval $(call iOSCrossTemplate,cross32,i386,arm,ios-target32,llvm36-llvm32,arm-apple-darwin10))
$(eval $(call iOSCrossTemplate,cross64,x86_64,aarch64,ios-target64,llvm-llvm64,aarch64-apple-darwin10))
ios-crosswatch_CONFIGURE_FLAGS=--enable-cooperative-suspend
$(eval $(call iOSCrossTemplate,crosswatch,i386,armv7k-unknown,ios-targetwatch,llvm36-llvm32,armv7k-apple-darwin))
# 64->arm32 cross compiler
$(eval $(call iOSCrossTemplate,cross32-64,x86_64,arm,ios-target32,llvm-llvm64,arm-apple-darwin10))

$(eval $(call BclTemplate,ios-bcl,monotouch monotouch_runtime monotouch_tv monotouch_tv_runtime monotouch_watch monotouch_watch_runtime,monotouch monotouch_tv monotouch_watch))
ios_TARGETS += ios-bcl
