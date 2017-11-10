
##
# Parameters:
#  $(1): arch
#  $(2): platform
#  $(3): abi_name
#  $(4): toolchain_name
#  $(5): host_triple
#
# Flags:
#  android_$(1)_CFLAGS
#  android_$(1)_CXXFLAGS
#  android_$(1)_LDFLAGS
define AndroidTemplate

_android_$(1)_AR=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-ar
_android_$(1)_AS=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-as
_android_$(1)_CC=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-clang
_android_$(1)_CPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-cpp
_android_$(1)_CXX=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-clang++
_android_$(1)_CXXCPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(3)-cpp
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
	-fno-omit-frame-pointer -Wl,-z,now -Wl,-z,relro -Wl,-z,noexecstack -fstack-protector $(if $(filter $(IS_RELEASE),true),-O2,-O0 -ggdb3) \
	-DMONODROID=1 \
	$$(android_$(1)_CFLAGS)

_android_$(1)_CXXFLAGS= \
	-fno-omit-frame-pointer -Wl,-z,now -Wl,-z,relro -Wl,-z,noexecstack -fstack-protector $(if $(filter $(IS_RELEASE),true),-O2,-O0 -ggdb3) \
	-DMONODROID=1 \
	$$(android_$(1)_CXXFLAGS)

_android_$(1)_LDFLAGS= \
	-ldl -lm -llog -lc -lgcc \
	-Wl,-rpath-link=$$(NDK_DIR)/platforms/$(2)/arch-$(1)/usr/lib,-dynamic-linker=/system/bin/linker \
	-L$$(NDK_DIR)/platforms/$(2)/arch-$(1)/usr/lib \
	$$(android_$(1)_LDFLAGS)

_android_$(1)_CONFIGURE_ENVIRONMENT = \
	AR="$$(_android_$(1)_AR)"	\
	AS="$$(_android_$(1)_AS)"	\
	CC="$$(_android_$(1)_CC)"	\
	CFLAGS="$$(_android_$(1)_CFLAGS)" \
	CPP="$$(_android_$(1)_CPP)"	\
	CXX="$$(_android_$(1)_CXX)" \
	CXXFLAGS="$$(_android_$(1)_CXXFLAGS) " \
	DLLTOOL="$$(_android_$(1)_DLLTOOL)" \
	LD="$$(_android_$(1)_LD)"	\
	LDFLAGS="$$(_android_$(1)_LDFLAGS)"	\
	OBJDUMP="$$(_android_$(1)_OBJDUMP)" \
	STRIP="$$(_android_$(1)_STRIP)" \
    CXXCPP="$$(_android_$(1)_CXXCPP)"	\
    RANLIB="$$(_android_$(1)_RANLIB)"

_android_$(1)_CONFIGURE_FLAGS= \
	--host=$(5) \
	--cache-file=$$(TOP)/sdks/builds/android-$(1).config.cache \
	--prefix=$$(TOP)/sdks/out/android-$(1) \
	--disable-boehm \
	--disable-executables \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--enable-dynamic-btls \
	--enable-maintainer-mode \
	--enable-minimal=ssa,portability,attach,verifier,full_messages,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging,security,shared_handles \
	--with-btls-android-ndk=$$(NDK_DIR) \
	--with-sigaltstack=yes \
	--with-tls=pthread \
	--without-ikvm-native

.stamp-android-$(1)-toolchain:
	$$(NDK_DIR)/build/tools/make-standalone-toolchain.sh  --platform=$(2) --arch=$(1) --install-dir=$$(TOP)/sdks/builds/toolchains/android-$(1) --toolchain=$(4)
	touch $$@

android-toolchain: .stamp-android-$(1)-toolchain

.stamp-android-$(1)-configure: $$(TOP)/configure .stamp-android-$(1)-toolchain
	mkdir -p $$(TOP)/sdks/builds/android-$(1)
	cd $$(TOP)/sdks/builds/android-$(1) && $$(TOP)/configure $$(_android_$(1)_AC_VARS) $$(_android_$(1)_CONFIGURE_ENVIRONMENT) $$(_android_$(1)_CONFIGURE_FLAGS)
	touch $$@

.PHONY: package-android-$(1)
package-android-$(1):
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)/mono install
	$$(MAKE) -C $$(TOP)/sdks/builds/android-$(1)/support install

.PHONY: clean-android-$(1)
clean-android-$(1)::
	rm -rf .stamp-android-$(1)-toolchain .stamp-android-$(1)-configure $$(TOP)/sdks/builds/toolchains/android-$(1) $$(TOP)/sdks/builds/android-$(1) $$(TOP)/sdks/builds/android-$(1).config.cache

TARGETS += android-$(1)

endef

## Android arm
android_arm_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -mtune=cortex-a8 -march=armv7-a -mfpu=vfp -mfloat-abi=softfp
android_arm_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic
android_arm_LDFLAGS=-Wl,--fix-cortex-a8
$(eval $(call AndroidTemplate,arm,android-14,arm-linux-androideabi,arm-linux-androideabi-clang,armv5-linux-androideabi,ARM))

## Android arm64
android_arm64_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
android_arm64_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic
$(eval $(call AndroidTemplate,arm64,android-21,aarch64-linux-android,aarch64-linux-android-clang,aarch64-linux-android,ARM64))

## Android x86
$(eval $(call AndroidTemplate,x86,android-14,i686-linux-android,x86-clang,i686-linux-android,X86))

## Android x86_64
android_x86_64_CFLAGS=-DL_cuserid=9
$(eval $(call AndroidTemplate,x86_64,android-21,x86_64-linux-android,x86_64-clang,x86_64-linux-android,X86_64))
