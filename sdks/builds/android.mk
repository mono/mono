
include runtime.mk

ANDROID_URI?=https://dl.google.com/android/repository/
ANDROID_TOOLCHAIN_PREFIX?=$(HOME)/android-toolchain/toolchains
ANDROID_TOOLCHAIN_DIR?=$(HOME)/android-toolchain
ANDROID_TOOLCHAIN_CACHE_DIR?=$(HOME)/android-archives

ANT_URI?=https://archive.apache.org/dist/ant/binaries/

$(ANDROID_TOOLCHAIN_CACHE_DIR):
	mkdir -p $@

##
# Parameters:
#  $(1): target
#  $(2): dir
#  $(3): category
#  $(4): url prefix
define AndroidProvisioningTemplate

$$(ANDROID_TOOLCHAIN_CACHE_DIR)/$(1).zip: | $$(ANDROID_TOOLCHAIN_CACHE_DIR)
	wget --no-verbose -O $$@ $(4)$(1).zip

$$(ANDROID_TOOLCHAIN_DIR)/$(3)$$(if $(2),/$(2))/.stamp-$(1): $$(ANDROID_TOOLCHAIN_CACHE_DIR)/$(1).zip
	rm -rf $$(ANDROID_TOOLCHAIN_DIR)/$(3)$$(if $(2),/$(2))
	./unzip-android-archive.sh "$$<" "$$(ANDROID_TOOLCHAIN_DIR)/$(3)$$(if $(2),/$(2))"
	touch $$@

.PHONY: provision-android-$(3)-$(1)
provision-android-$(3)-$(1): $$(ANDROID_TOOLCHAIN_DIR)/$(3)$$(if $(2),/$(2))/.stamp-$(1)

.PHONY: provision-android
provision-android: provision-android-$(3)-$(1)

endef

AndroidNDKProvisioningTemplate=$(call AndroidProvisioningTemplate,$(1),,ndk,$(ANDROID_URI))

ifeq ($(UNAME),Darwin)
$(eval $(call AndroidNDKProvisioningTemplate,android-ndk-$(ANDROID_NDK_VERSION)-darwin-x86_64))
else
ifeq ($(UNAME),Linux)
$(eval $(call AndroidNDKProvisioningTemplate,android-ndk-$(ANDROID_NDK_VERSION)-linux-x86_64))
else
$(error "Unknown UNAME=$(UNAME)")
endif
endif

AndroidSDKProvisioningTemplate=$(call AndroidProvisioningTemplate,$(1),$(2),sdk,$(ANDROID_URI)$(3))

ifeq ($(UNAME),Darwin)
$(eval $(call AndroidSDKProvisioningTemplate,build-tools_r$(ANDROID_BUILD_TOOLS_VERSION)-macosx,build-tools/$(or $(ANDROID_BUILD_TOOLS_DIR),$(ANDROID_BUILD_TOOLS_VERSION))))
$(eval $(call AndroidSDKProvisioningTemplate,platform-tools_r27.0.1-darwin,platform-tools))
$(eval $(call AndroidSDKProvisioningTemplate,sdk-tools-darwin-4333796,tools))
$(eval $(call AndroidSDKProvisioningTemplate,emulator-darwin-4266726,emulator))
$(eval $(call AndroidSDKProvisioningTemplate,cmake-3.6.4111459-darwin-x86_64,cmake/3.6.4111459))
else
ifeq ($(UNAME),Linux)
$(eval $(call AndroidSDKProvisioningTemplate,build-tools_r$(ANDROID_BUILD_TOOLS_VERSION)-linux,build-tools/$(or $(ANDROID_BUILD_TOOLS_DIR),$(ANDROID_BUILD_TOOLS_VERSION))))
$(eval $(call AndroidSDKProvisioningTemplate,platform-tools_r27.0.1-linux,platform-tools))
$(eval $(call AndroidSDKProvisioningTemplate,sdk-tools-linux-4333796,tools))
$(eval $(call AndroidSDKProvisioningTemplate,emulator-linux-4266726,emulator))
$(eval $(call AndroidSDKProvisioningTemplate,cmake-3.6.4111459-linux-x86_64,cmake/3.6.4111459))
else
$(error "Unknown UNAME=$(UNAME)")
endif
endif

$(eval $(call AndroidSDKProvisioningTemplate,android-2.3.3_r02-linux,platforms/android-10))
$(eval $(call AndroidSDKProvisioningTemplate,android-15_r03,platforms/android-15))
$(eval $(call AndroidSDKProvisioningTemplate,android-16_r04,platforms/android-16))
$(eval $(call AndroidSDKProvisioningTemplate,android-17_r02,platforms/android-17))
$(eval $(call AndroidSDKProvisioningTemplate,android-18_r02,platforms/android-18))
$(eval $(call AndroidSDKProvisioningTemplate,android-19_r03,platforms/android-19))
$(eval $(call AndroidSDKProvisioningTemplate,android-20_r02,platforms/android-20))
$(eval $(call AndroidSDKProvisioningTemplate,android-21_r02,platforms/android-21))
$(eval $(call AndroidSDKProvisioningTemplate,android-22_r02,platforms/android-22))
$(eval $(call AndroidSDKProvisioningTemplate,platform-23_r03,platforms/android-23))
$(eval $(call AndroidSDKProvisioningTemplate,platform-24_r02,platforms/android-24))
$(eval $(call AndroidSDKProvisioningTemplate,platform-25_r03,platforms/android-25))
$(eval $(call AndroidSDKProvisioningTemplate,platform-26_r02,platforms/android-26))
$(eval $(call AndroidSDKProvisioningTemplate,platform-27_r03,platforms/android-27))
$(eval $(call AndroidSDKProvisioningTemplate,platform-28_r04,platforms/android-28))
$(eval $(call AndroidSDKProvisioningTemplate,docs-24_r01,docs))
$(eval $(call AndroidSDKProvisioningTemplate,android_m2repository_r16,extras/android/m2repository))
$(eval $(call AndroidSDKProvisioningTemplate,x86-21_r05,system-images/android-21/default/x86,sys-img/android/))

AndroidAntProvisioningTemplate=$(call AndroidProvisioningTemplate,$(1),,ant,$(ANT_URI))

$(eval $(call AndroidAntProvisioningTemplate,apache-ant-1.9.9-bin))

##
# Parameters:
#  $(1): target
#  $(2): arch
#  $(3): abi_name
#  $(4): host_triple
#
# Flags:
#  android-$(1)_CFLAGS
#  android-$(1)_CXXFLAGS
#  android-$(1)_LDFLAGS
define AndroidTargetTemplate

_android-$(1)_AR=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-ar
_android-$(1)_AS=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-as
_android-$(1)_CC=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-clang
_android-$(1)_CXX=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-clang++
_android-$(1)_CPP=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-cpp
_android-$(1)_CXXCPP=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-cpp
_android-$(1)_DLLTOOL=
_android-$(1)_LD=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-ld
_android-$(1)_OBJDUMP="$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-objdump"
_android-$(1)_RANLIB=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-ranlib
_android-$(1)_STRIP=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-strip

_android-$(1)_AC_VARS= \
	mono_cv_uscore=yes \
	ac_cv_func_sched_getaffinity=no \
	ac_cv_func_sched_setaffinity=no \
	ac_cv_func_shm_open_working_with_mmap=no

_android-$(1)_CFLAGS= \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android-$(1)_CFLAGS)

_android-$(1)_CXXFLAGS= \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android-$(1)_CXXFLAGS)

_android-$(1)_CPPFLAGS= \
	-I$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/usr/include

_android-$(1)_CXXCPPFLAGS= \
	-I$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/usr/include

_android-$(1)_LDFLAGS= \
	-z now -z relro -z noexecstack \
	-ldl -lm -llog -lc -lgcc \
	-Wl,-rpath-link=$$(ANDROID_TOOLCHAIN_DIR)/ndk/platforms/android-$$(ANDROID_SDK_VERSION_$(1))/arch-$(2)/usr/lib,-dynamic-linker=/system/bin/linker \
	-L$$(ANDROID_TOOLCHAIN_DIR)/ndk/platforms/android-$$(ANDROID_SDK_VERSION_$(1))/arch-$(2)/usr/lib \
	$$(android-$(1)_LDFLAGS)

_android-$(1)_CONFIGURE_FLAGS= \
	--host=$(4) \
	--cache-file=$$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION).config.cache \
	--prefix=$$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION) \
	--disable-boehm \
	--disable-executables \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--enable-dynamic-btls \
	--enable-maintainer-mode \
	--enable-minimal=ssa,portability,attach,verifier,full_messages,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging,security,shared_handles,interpreter \
	--enable-monodroid \
	--with-btls-android-ndk=$$(ANDROID_TOOLCHAIN_DIR)/ndk \
	--with-sigaltstack=yes \
	--with-tls=pthread \
	--without-ikvm-native

.stamp-android-$(1)-toolchain: | $$(if $$(IGNORE_PROVISION_ANDROID),,provision-android)
	python "$$(ANDROID_TOOLCHAIN_DIR)/ndk/build/tools/make_standalone_toolchain.py" --verbose --force --api=$$(ANDROID_SDK_VERSION_$(1)) --arch=$(2) --install-dir=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang
	touch $$@

$$(eval $$(call RuntimeTemplate,android-$(1)))

endef

## android-armeabi
android-armeabi_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv5te
android-armeabi_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv5te
android-armeabi_LDFLAGS=-Wl,--fix-cortex-a8
$(eval $(call AndroidTargetTemplate,armeabi,arm,arm-linux-androideabi,armv5-linux-androideabi))

## android-armeabi-v7a
android-armeabi-v7a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android-armeabi-v7a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android-armeabi-v7a_LDFLAGS=-Wl,--fix-cortex-a8
$(eval $(call AndroidTargetTemplate,armeabi-v7a,arm,arm-linux-androideabi,armv5-linux-androideabi))

## android-arm64-v8a
android-arm64-v8a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
android-arm64-v8a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
$(eval $(call AndroidTargetTemplate,arm64-v8a,arm64,aarch64-linux-android,aarch64-linux-android))

## android-x86
$(eval $(call AndroidTargetTemplate,x86,x86,i686-linux-android,i686-linux-android))

## android-x86_64
android-x86_64_CFLAGS=-DL_cuserid=9
android-x86_64_CXXFLAGS=-DL_cuserid=9
$(eval $(call AndroidTargetTemplate,x86_64,x86_64,x86_64-linux-android,x86_64-linux-android))

##
# Parameters
#  $(1): target
#
# Flags:
#  android-$(1)_CFLAGS
#  android-$(1)_CXXFLAGS
define AndroidHostTemplate

_android-$(1)_AR=ar
_android-$(1)_AS=as
_android-$(1)_CC=cc
_android-$(1)_CXX=c++
_android-$(1)_CXXCPP=cpp
_android-$(1)_LD=ld
_android-$(1)_RANLIB=ranlib
_android-$(1)_STRIP=strip

_android-$(1)_CFLAGS=$$(android-$(1)_CFLAGS)
_android-$(1)_CXXFLAGS=$$(android-$(1)_CXXFLAGS)

_android-$(1)_CONFIGURE_FLAGS= \
	--cache-file=$$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION).config.cache \
	--prefix=$$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION) \
	--disable-boehm \
	--disable-iconv \
	--disable-mono-debugger \
	--disable-nls \
	--enable-dynamic-btls \
	--enable-maintainer-mode \
	--enable-monodroid \
	--with-mcs-docs=no \
	--with-monodroid \
	--with-profile4_x=no \
	--without-ikvm-native

.stamp-android-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,android-$(1)))

endef

android-host-Darwin_CFLAGS=-mmacosx-version-min=10.9
$(eval $(call AndroidHostTemplate,host-Darwin))
$(eval $(call AndroidHostTemplate,host-Linux))

##
# Parameters
#  $(1): target
#  $(2): arch
define AndroidHostMxeTemplate

_android-$(1)_PATH=$$(MXE_PREFIX)/bin

_android-$(1)_AR=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-ar
_android-$(1)_AS=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-as
_android-$(1)_CC=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-gcc
_android-$(1)_CXX=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-g++
_android-$(1)_DLLTOOL=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-dlltool
_android-$(1)_LD=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-ld
_android-$(1)_OBJDUMP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-objdump
_android-$(1)_RANLIB=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-ranlib
_android-$(1)_STRIP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static)-strip

_android-$(1)_AC_VARS= \
	ac_cv_header_zlib_h=no \
	ac_cv_search_dlopen=no

_android-$(1)_CFLAGS= \
	-DXAMARIN_PRODUCT_VERSION=0

_android-$(1)_CXXFLAGS= \
	-DXAMARIN_PRODUCT_VERSION=0

_android-$(1)_CONFIGURE_FLAGS= \
	--host=$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static) \
	--target=$(2)-w64-mingw32$$(if $$(filter $(UNAME),Darwin),.static) \
	--cache-file=$$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION).config.cache \
	--prefix=$$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION) \
	--disable-boehm \
	--disable-llvm \
	--disable-mcs-build \
	--disable-nls \
	--enable-maintainer-mode \
	--enable-monodroid \
	--with-monodroid

.stamp-android-$(1)-toolchain:
	touch $$@

.stamp-android-$(1)-$$(CONFIGURATION)-configure: | $(if $(IGNORE_PROVISION_MXE),,provision-mxe)

$$(eval $$(call RuntimeTemplate,android-$(1)))

endef

ifneq ($(MXE_PREFIX),)
$(eval $(call AndroidHostMxeTemplate,host-mxe-Win32,i686))
$(eval $(call AndroidHostMxeTemplate,host-mxe-Win64,x86_64))
endif
