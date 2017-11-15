
##
# Parameters:
#  $(1): target
#  $(2): arch
#  $(3): platform
#  $(4): abi_name
#  $(5): toolchain_name
#  $(6): host_triple
#
# Flags:
#  android_$(1)_CFLAGS
#  android_$(1)_CXXFLAGS
#  android_$(1)_LDFLAGS
define AndroidTargetTemplate

_android_$(1)_AR=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-ar
_android_$(1)_AS=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-as
_android_$(1)_CC=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-clang
_android_$(1)_CXX=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-clang++
_android_$(1)_CPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-cpp -I$$(TOP)/sdks/builds/toolchains/android-$(1)/usr/include
_android_$(1)_CXXCPP=$$(CCACHE) $$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-cpp -I$$(TOP)/sdks/builds/toolchains/android-$(1)/usr/include
_android_$(1)_DLLTOOL=
_android_$(1)_LD=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-ld
_android_$(1)_OBJDUMP="$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-objdump"
_android_$(1)_RANLIB=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-ranlib
_android_$(1)_STRIP=$$(TOP)/sdks/builds/toolchains/android-$(1)/bin/$(4)-strip

_android_$(1)_AC_VARS= \
	mono_cv_uscore=yes \
	ac_cv_func_sched_getaffinity=no \
	ac_cv_func_sched_setaffinity=no

_android_$(1)_CFLAGS= \
	$(if $(filter $(IS_RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android_$(1)_CFLAGS)

_android_$(1)_CXXFLAGS= \
	$(if $(filter $(IS_RELEASE),true),-O2 -g,-O0 -ggdb3 -fno-omit-frame-pointer) \
	-fstack-protector \
	-DMONODROID=1 \
	$$(android_$(1)_CXXFLAGS)

_android_$(1)_LDFLAGS= \
	-z now -z relro -z noexecstack \
	-ldl -lm -llog -lc -lgcc \
	-Wl,-rpath-link=$$(NDK_DIR)/platforms/$(3)/arch-$(1)/usr/lib,-dynamic-linker=/system/bin/linker \
	-L$$(NDK_DIR)/platforms/$(3)/arch-$(1)/usr/lib \
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
	--host=$(6) \
	--cache-file=$$(TOP)/sdks/builds/android-$(1).config.cache \
	--prefix=$$(TOP)/sdks/out/android-$(1) \
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
	$$(NDK_DIR)/build/tools/make-standalone-toolchain.sh  --platform=$(3) --arch=$(2) --install-dir=$$(TOP)/sdks/builds/toolchains/android-$(1) --toolchain=$(5)
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

## android-armeabi
android_armeabi_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv5te
android_armeabi_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv5te
android_armeabi_LDFLAGS=-Wl,--fix-cortex-a8
$(eval $(call AndroidTargetTemplate,armeabi,arm,android-9,arm-linux-androideabi,arm-linux-androideabi-clang,armv5-linux-androideabi))

## Android armv7
android_armeabi-v7a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android_armeabi-v7a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android_armeabi-v7a_LDFLAGS=-Wl,--fix-cortex-a8
$(eval $(call AndroidTargetTemplate,armeabi-v7a,arm,android-9,arm-linux-androideabi,arm-linux-androideabi-clang,armv5-linux-androideabi))

## Android arm64
android_arm64-v8a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
android_arm64-v8a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
$(eval $(call AndroidTargetTemplate,arm64-v8a,arm64,android-21,aarch64-linux-android,aarch64-linux-android-clang,aarch64-linux-android))

## Android x86
$(eval $(call AndroidTargetTemplate,x86,x86,android-14,i686-linux-android,x86-clang,i686-linux-android))

## Android x86_64
android_x86_64_CFLAGS=-DL_cuserid=9
android_x86_64_CXXFLAGS=-DL_cuserid=9
$(eval $(call AndroidTargetTemplate,x86_64,x86_64,android-21,x86_64-linux-android,x86_64-clang,x86_64-linux-android))
