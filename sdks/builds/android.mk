
##
# Parameters:
#  $(1): target
#  $(2): arch
#  $(3): abi_name
#  $(4): host_triple
#
# Flags:
#  android_$(1)_CFLAGS
#  android_$(1)_CXXFLAGS
#  android_$(1)_LDFLAGS
define AndroidTargetTemplate

_android_$(1)_AR=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-ar
_android_$(1)_AS=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-as
_android_$(1)_CC=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-clang
_android_$(1)_CXX=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-clang++
_android_$(1)_CPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-cpp -I$$(TOP)/sdks/builds/toolchains/android-$(1)/usr/include
_android_$(1)_CXXCPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-cpp -I$$(TOP)/sdks/builds/toolchains/android-$(1)/usr/include
_android_$(1)_DLLTOOL=
_android_$(1)_LD=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-ld
_android_$(1)_OBJDUMP="$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-objdump"
_android_$(1)_RANLIB=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-ranlib
_android_$(1)_STRIP=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-strip

_android_$(1)_AC_VARS= \
	mono_cv_uscore=yes \
	ac_cv_func_sched_getaffinity=no \
	ac_cv_func_sched_setaffinity=no

_android_$(1)_CFLAGS= \
	$(if $(filter $(RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android_$(1)_CFLAGS)

_android_$(1)_CXXFLAGS= \
	$(if $(filter $(RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android_$(1)_CXXFLAGS)

_android_$(1)_LDFLAGS= \
	-z now -z relro -z noexecstack \
	-ldl -lm -llog -lc -lgcc \
	-Wl,-rpath-link=$$(NDK_DIR)/platforms/android-$$(ANDROID_PLATFORM_VERSION_$(1))/arch-$(2)/usr/lib,-dynamic-linker=/system/bin/linker \
	-L$$(NDK_DIR)/platforms/android-$$(ANDROID_PLATFORM_VERSION_$(1))/arch-$(2)/usr/lib \
	$$(android_$(1)_LDFLAGS)

_android_$(1)_CONFIGURE_ENVIRONMENT = \
	AR="$$(_android_$(1)_AR)"	\
	AS="$$(_android_$(1)_AS)"	\
	CC="$$(_android_$(1)_CC)"	\
	CFLAGS="$$(_android_$(1)_CFLAGS)" \
	CXX="$$(_android_$(1)_CXX)" \
	CXXFLAGS="$$(_android_$(1)_CXXFLAGS) " \
	CPP="$$(_android_$(1)_CPP) $$(_android_$(1)_CPPFLAGS)"	\
	CXXCPP="$$(_android_$(1)_CXXCPP)"	\
	DLLTOOL="$$(_android_$(1)_DLLTOOL)" \
	LD="$$(_android_$(1)_LD)"	\
	LDFLAGS="$$(_android_$(1)_LDFLAGS)"	\
	OBJDUMP="$$(_android_$(1)_OBJDUMP)" \
	STRIP="$$(_android_$(1)_STRIP)" \
	RANLIB="$$(_android_$(1)_RANLIB)"

_android_$(1)_CONFIGURE_FLAGS= \
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
	python "$$(NDK_DIR)/build/tools/make_standalone_toolchain.py" --verbose --force --api=$$(ANDROID_PLATFORM_VERSION_$(1)) --arch=$(2) --install-dir=$$(TOP)/sdks/builds/toolchains/android-$(1)
	touch $$@

.stamp-android-$(1)-$$(CONFIGURATION)-configure: $$(TOP)/configure .stamp-android-$(1)-toolchain
	mkdir -p $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)
	cd $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION) && $$(TOP)/configure $$(_android_$(1)_AC_VARS) $$(_android_$(1)_CONFIGURE_ENVIRONMENT) $$(_android_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: .stamp-android-$(1)-configure
.stamp-android-$(1)-configure: .stamp-android-$(1)-$$(CONFIGURATION)-configure

.PHONY: build-custom-android-$(1)
build-custom-android-$(1):
	$$(MAKE) -C android-$(1)-$$(CONFIGURATION)

.PHONY: setup-custom-android-$(1)
setup-custom-android-$(1):
	mkdir -p $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)

.PHONY: package-android-$(1)-$$(CONFIGURATION)
package-android-$(1)-$$(CONFIGURATION):
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/mono install
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/support install

.PHONY: package-android-$(1)
package-android-$(1):
	$$(MAKE) package-android-$(1)-$$(CONFIGURATION)

.PHONY: clean-android-$(1)-$$(CONFIGURATION)
clean-android-$(1)-$$(CONFIGURATION):
	rm -rf .stamp-android-$(1)-toolchain .stamp-android-$(1)-$$(CONFIGURATION)-configure $$(TOP)/sdks/builds/toolchains/android-$(1) $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION) $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION).config.cache $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)

.PHONY: clean-android-$(1)
clean-android-$(1):
	$$(MAKE) clean-android-$(1)-$$(CONFIGURATION)

TARGETS += android-$(1)

endef

## android-armeabi
android_armeabi_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv5te
android_armeabi_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv5te
android_armeabi_LDFLAGS=-Wl,--fix-cortex-a8
$(eval $(call AndroidTargetTemplate,armeabi,arm,arm-linux-androideabi,armv5-linux-androideabi))

## android-armeabi-v7a
android_armeabi-v7a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android_armeabi-v7a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android_armeabi-v7a_LDFLAGS=-Wl,--fix-cortex-a8
$(eval $(call AndroidTargetTemplate,armeabi-v7a,arm,arm-linux-androideabi,armv5-linux-androideabi))

## android-arm64-v8a
android_arm64-v8a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
android_arm64-v8a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
$(eval $(call AndroidTargetTemplate,arm64-v8a,arm64,aarch64-linux-android,aarch64-linux-android))

## android-x86
$(eval $(call AndroidTargetTemplate,x86,x86,i686-linux-android,i686-linux-android))

## android-x86_64
android_x86_64_CFLAGS=-DL_cuserid=9
android_x86_64_CXXFLAGS=-DL_cuserid=9
$(eval $(call AndroidTargetTemplate,x86_64,x86_64,x86_64-linux-android,x86_64-linux-android))

##
# Parameters
#  $(1): target
#
# Flags:
#  android_$(1)_CFLAGS
#  android_$(1)_CXXFLAGS
#
# Notes:
#  XA doesn't seem to build differently for Darwin and Linux, seems like a bug on their end
define AndroidHostTemplate

_android_$(1)_AR=ar
_android_$(1)_AS=as
_android_$(1)_CC=cc
_android_$(1)_CXX=c++
_android_$(1)_CXXCPP=cpp
_android_$(1)_LD=ld
_android_$(1)_RANLIB=ranlib
_android_$(1)_STRIP=strip

_android_$(1)_CFLAGS= \
	$(if $(filter $(RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	$$(android_$(1)_CFLAGS)

_android_$(1)_CXXFLAGS= \
	$(if $(filter $(RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	$$(android_$(1)_CXXFLAGS)

_android_$(1)_CONFIGURE_ENVIRONMENT= \
	AR="$$(_android_$(1)_AR)" \
	AS="$$(_android_$(1)_AS)" \
	CC="$$(_android_$(1)_CC)" \
	CFLAGS="$$(_android_$(1)_CFLAGS)" \
	CXX="$$(_android_$(1)_CXX)" \
	CXXFLAGS="$$(_android_$(1)_CXXFLAGS)" \
	CXXCPP="$$(_android_$(1)_CXXCPP)" \
	LD="$$(_android_$(1)_LD)" \
	RANLIB="$$(_android_$(1)_RANLIB)" \
	STRIP="$$(_android_$(1)_STRIP)"

_android_$(1)_CONFIGURE_FLAGS= \
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

.stamp-android-$(1)-$$(CONFIGURATION)-configure: $$(TOP)/configure .stamp-android-$(1)-toolchain
	mkdir -p $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)
	cd $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION) && $$< $$(_android_$(1)_CONFIGURE_ENVIRONMENT) $$(_android_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: .stamp-android-$(1)-configure
.stamp-android-$(1)-configure: .stamp-android-$(1)-$$(CONFIGURATION)-configure

.PHONY: build-custom-android-$(1)
build-custom-android-$(1):
	$$(MAKE) -C android-$(1)-$$(CONFIGURATION)

.PHONY: setup-custom-android-$(1)
setup-custom-android-$(1):
	mkdir -p $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)

.PHONY: package-android-$(1)-$$(CONFIGURATION)
package-android-$(1)-$$(CONFIGURATION):
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/mono install
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/support install

.PHONY: package-android-$(1)
package-android-$(1):
	$$(MAKE) package-android-$(1)-$$(CONFIGURATION)

.PHONY: clean-android-$(1)-$$(CONFIGURATION)
clean-android-$(1)-$$(CONFIGURATION):
	rm -rf .stamp-android-$(1)-toolchain .stamp-android-$(1)-$$(CONFIGURATION)-configure $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION) $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION).config.cache $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)

.PHONY: clean-android-$(1)
clean-android-$(1):
	$$(MAKE) clean-android-$(1)-$$(CONFIGURATION)

TARGETS += android-$(1)

endef

android_host-Darwin_CFLAGS=-mmacosx-version-min=10.9
$(eval $(call AndroidHostTemplate,host-Darwin))
$(eval $(call AndroidHostTemplate,host-Linux))

##
# Parameters
#  $(1): target
#  $(2): arch
#  $(3): mxe
define AndroidHostMxeTemplate

_android_$(1)_PATH=$$(TOP)/sdks/out/mxe-$(3)/bin

_android_$(1)_AR=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-ar
_android_$(1)_AS=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-as
_android_$(1)_CC=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-gcc
_android_$(1)_CXX=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-g++
_android_$(1)_DLLTOOL=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-dlltool
_android_$(1)_LD=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-ld
_android_$(1)_OBJDUMP=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-objdump
_android_$(1)_RANLIB=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-ranlib
_android_$(1)_STRIP=$$(TOP)/sdks/out/mxe-$(3)/bin/$(2)-w64-mingw32.static-strip

_android_$(1)_AC_VARS= \
	ac_cv_header_zlib_h=no \
	ac_cv_search_dlopen=no

_android_$(1)_CFLAGS= \
	$(if $(filter $(RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	-DXAMARIN_PRODUCT_VERSION=0

_android_$(1)_CXXFLAGS= \
	$(if $(filter $(RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	-DXAMARIN_PRODUCT_VERSION=0

_android_$(1)_CONFIGURE_ENVIRONMENT= \
	AR="$$(_android_$(1)_AR)" \
	AS="$$(_android_$(1)_AS)" \
	CC="$$(_android_$(1)_CC)" \
	CFLAGS="$$(_android_$(1)_CFLAGS)" \
	CXX="$$(_android_$(1)_CXX)" \
	CXXFLAGS="$$(_android_$(1)_CXXFLAGS)" \
	LD="$$(_android_$(1)_LD)" \
	RANLIB="$$(_android_$(1)_RANLIB)" \
	STRIP="$$(_android_$(1)_STRIP)"

_android_$(1)_CONFIGURE_FLAGS= \
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

.stamp-android-$(1)-$$(CONFIGURATION)-configure: $$(TOP)/configure .stamp-android-$(1)-toolchain | package-mxe-$(3)
	mkdir -p $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)
	cd $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION) && PATH="$$$$PATH:$$(_android_$(1)_PATH)" $$< $$(_android_$(1)_AC_VARS) $$(_android_$(1)_CONFIGURE_ENVIRONMENT) $$(_android_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: .stamp-android-$(1)-configure
.stamp-android-$(1)-configure: .stamp-android-$(1)-$$(CONFIGURATION)-configure

.PHONY: build-custom-android-$(1)
build-custom-android-$(1):
	$$(MAKE) -C android-$(1)-$$(CONFIGURATION)

.PHONY: setup-custom-android-$(1)
setup-custom-android-$(1):
	mkdir -p $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)

.PHONY: package-android-$(1)-$$(CONFIGURATION)
package-android-$(1)-$$(CONFIGURATION):
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/mono install
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION)/support install

.PHONY: package-android-$(1)
package-android-$(1):
	$$(MAKE) package-android-$(1)-$$(CONFIGURATION)

.PHONY: clean-android-$(1)-$$(CONFIGURATION)
clean-android-$(1)-$$(CONFIGURATION):
	rm -rf .stamp-android-$(1)-toolchain .stamp-android-$(1)-$$(CONFIGURATION)-configure $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION) $$(TOP)/sdks/builds/android-$(1)-$$(CONFIGURATION).config.cache $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)

.PHONY: clean-android-$(1)
clean-android-$(1):
	$$(MAKE) clean-android-$(1)-$$(CONFIGURATION)

TARGETS += android-$(1)

endef

$(eval $(call AndroidHostMxeTemplate,host-mxe-Win32,i686,Win32))
$(eval $(call AndroidHostMxeTemplate,host-mxe-Win64,x86_64,Win64))
