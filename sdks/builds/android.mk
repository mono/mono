
include runtime.mk

ANDROID_TOOLCHAIN_PREFIX?=$(TOP)/sdks/builds/toolchains/android

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

<<<<<<< HEAD
_android-$(1)_AR=$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-ar
_android-$(1)_AS=$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-as
_android-$(1)_CC=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-clang
_android-$(1)_CXX=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-clang++
_android-$(1)_CPP=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-cpp
_android-$(1)_CXXCPP=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-cpp
_android-$(1)_DLLTOOL=
_android-$(1)_LD=$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-ld
_android-$(1)_OBJDUMP="$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-objdump"
_android-$(1)_RANLIB=$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-ranlib
_android-$(1)_STRIP=$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/bin/$(3)-strip
=======
_android-$(1)_AR=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-ar
_android-$(1)_AS=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-as
_android-$(1)_CC=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-clang
_android-$(1)_CXX=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-clang++
_android-$(1)_CPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-cpp -I$$(TOP)/sdks/builds/toolchains/android-$(1)/usr/include
_android-$(1)_CXXCPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-cpp -I$$(TOP)/sdks/builds/toolchains/android-$(1)/usr/include
_android-$(1)_DLLTOOL=
_android-$(1)_LD=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-ld
_android-$(1)_OBJDUMP="$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-objdump"
_android-$(1)_RANLIB=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-ranlib
_android-$(1)_STRIP=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-strip
>>>>>>> 43ac34b36e2... [sdks] Unify targets to build runtimes (#7394)

_android-$(1)_AC_VARS= \
	mono_cv_uscore=yes \
	ac_cv_func_sched_getaffinity=no \
	ac_cv_func_sched_setaffinity=no

_android-$(1)_CFLAGS= \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android-$(1)_CFLAGS)

_android-$(1)_CXXFLAGS= \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android-$(1)_CXXFLAGS)
<<<<<<< HEAD

_android-$(1)_CPPFLAGS= \
	-I$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/usr/include

_android-$(1)_CXXCPPFLAGS= \
	-I$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang/usr/include

=======

>>>>>>> 43ac34b36e2... [sdks] Unify targets to build runtimes (#7394)
_android-$(1)_LDFLAGS= \
	-z now -z relro -z noexecstack \
	-ldl -lm -llog -lc -lgcc \
	-Wl,-rpath-link=$$(NDK_DIR)/platforms/android-$$(ANDROID_PLATFORM_VERSION_$(1))/arch-$(2)/usr/lib,-dynamic-linker=/system/bin/linker \
	-L$$(NDK_DIR)/platforms/android-$$(ANDROID_PLATFORM_VERSION_$(1))/arch-$(2)/usr/lib \
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
	--with-btls-android-ndk=$$(NDK_DIR) \
	--with-sigaltstack=yes \
	--with-tls=pthread \
	--without-ikvm-native

.stamp-android-$(1)-toolchain:
	python "$$(NDK_DIR)/build/tools/make_standalone_toolchain.py" --verbose --force --api=$$(ANDROID_PLATFORM_VERSION_$(1)) --arch=$(2) --install-dir=$$(ANDROID_TOOLCHAIN_PREFIX)/$(3)-clang
	touch $$@

<<<<<<< HEAD
=======
.PHONY: package-android-$(1)-$$(CONFIGURATION)
package-android-$(1)-$$(CONFIGURATION)::
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/support install

>>>>>>> 43ac34b36e2... [sdks] Unify targets to build runtimes (#7394)
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
	--with-mcs-docs=no \
	--with-monodroid \
	--with-profile4_x=no \
	--without-ikvm-native

.stamp-android-$(1)-toolchain:
	touch $$@

<<<<<<< HEAD
=======
.PHONY: package-android-$(1)-$$(CONFIGURATION)
package-android-$(1)-$$(CONFIGURATION)::
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/support install

>>>>>>> 43ac34b36e2... [sdks] Unify targets to build runtimes (#7394)
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

_android-$(1)_PATH=$$(TOP)/sdks/out/mxe/bin

_android-$(1)_AR=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-ar
_android-$(1)_AS=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-as
_android-$(1)_CC=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-gcc
_android-$(1)_CXX=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-g++
_android-$(1)_DLLTOOL=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-dlltool
_android-$(1)_LD=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-ld
_android-$(1)_OBJDUMP=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-objdump
_android-$(1)_RANLIB=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-ranlib
_android-$(1)_STRIP=$$(TOP)/sdks/out/mxe/bin/$(2)-w64-mingw32.static-strip

_android-$(1)_AC_VARS= \
	ac_cv_header_zlib_h=no \
	ac_cv_search_dlopen=no

_android-$(1)_CFLAGS= \
	-DXAMARIN_PRODUCT_VERSION=0

_android-$(1)_CXXFLAGS= \
	-DXAMARIN_PRODUCT_VERSION=0

_android-$(1)_CONFIGURE_FLAGS= \
	--host=$(2)-w64-mingw32.static \
	--target=$(2)-w64-mingw32.static \
	--cache-file=$$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION).config.cache \
	--prefix=$$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION) \
	--disable-boehm \
	--disable-llvm \
	--disable-mcs-build \
	--disable-nls \
	--enable-maintainer-mode \
	--with-monodroid

.stamp-android-$(1)-toolchain:
	touch $$@

.stamp-android-$(1)-$$(CONFIGURATION)-configure: | package-mxe

<<<<<<< HEAD
=======
.PHONY: package-android-$(1)-$$(CONFIGURATION)
package-android-$(1)-$$(CONFIGURATION)::
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/support install

>>>>>>> 43ac34b36e2... [sdks] Unify targets to build runtimes (#7394)
$$(eval $$(call RuntimeTemplate,android-$(1)))

endef

$(eval $(call AndroidHostMxeTemplate,host-mxe-Win32,i686))
$(eval $(call AndroidHostMxeTemplate,host-mxe-Win64,x86_64))
