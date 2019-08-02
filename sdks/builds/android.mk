
ANDROID_URI?=https://dl.google.com/android/repository/
ANT_URI?=https://archive.apache.org/dist/ant/binaries/

ANDROID_SDK_PREFIX?=$(ANDROID_TOOLCHAIN_DIR)/sdk
ANDROID_TOOLCHAIN_PREFIX?=$(ANDROID_TOOLCHAIN_DIR)/toolchains
ANDROID_NEW_NDK=$(shell if test `grep 'Pkg\.Revision' $(ANDROID_TOOLCHAIN_DIR)/ndk/source.properties | cut -d '=' -f 2 | tr -d ' ' | cut -d '.' -f 1` -ge 18; then echo yes; else echo no; fi)

android_SOURCES_DIR = $(TOP)/sdks/out/android-sources
android_TPN_DIR = $(TOP)/sdks/out/android-tpn
android_HOST_DARWIN_LIB_DIR = $(TOP)/sdks/out/android-host-Darwin-$(CONFIGURATION)/lib
android_HOST_DARWIN_BIN_DIR = $(TOP)/sdks/out/android-host-Darwin-$(CONFIGURATION)/bin
android_PLATFORM_BIN=$(XCODE_DIR)/Toolchains/XcodeDefault.xctoolchain/usr/bin

ifneq (,$(filter $(UNAME),Darwin Linux))
android_ARCHIVE += android-sources android-tpn
ADDITIONAL_PACKAGE_DEPS += $(android_SOURCES_DIR) $(android_TPN_DIR)
endif

ifeq ($(UNAME),Darwin)
ANDROID_LIBCLANG = $(ANDROID_TOOLCHAIN_DIR)/ndk/toolchains/llvm/prebuilt/darwin-x86_64/lib64/libclang.dylib
else ifeq ($(UNAME),Linux)
ANDROID_LIBCLANG = $(ANDROID_TOOLCHAIN_DIR)/ndk/toolchains/llvm/prebuilt/linux-x86_64/lib64/libclang.so.8svn
endif

##
# Parameters:
#  $(1): target
#  $(2): dir
#  $(3): subdir (optional)
#  $(4): stamp (optional, default to $(1))
define AndroidProvisioningTemplate

$$(ANDROID_TOOLCHAIN_CACHE_DIR)/$(1).zip:
	mkdir -p $$(dir $$@)
	wget --no-verbose -O $$@ $$(ANDROID_URI)$(1).zip

$$(ANDROID_TOOLCHAIN_DIR)/$(2)/.stamp-$$(or $(4),$(1)): $$(ANDROID_TOOLCHAIN_CACHE_DIR)/$(1).zip
	mkdir -p $$(dir $$@)
	rm -rf $$(ANDROID_TOOLCHAIN_DIR)/$(2)$$(if $(3),/$(3))
	./unzip-android-archive.sh "$$<" "$$(ANDROID_TOOLCHAIN_DIR)/$(2)$$(if $(3),/$(3))"
	touch $$@

.PHONY: provision-android
provision-android: $$(ANDROID_TOOLCHAIN_DIR)/$(2)/.stamp-$$(or $(4),$(1))

endef

##
# Parameters:
# $(1): target
# $(2): dir
# $(3): subdir (optional)
# $(4): stamp (optional, default to $(1))
define AndroidProvisioningTemplateStub

.PHONY: provision-android-$(1)
provision-android-$(1):
	@echo "TODO: provision $(2)/.stamp-$$(or $(4),$(1)) for $$(UNAME)"

.PHONY: provision-android
provision-android: provision-android-$(1)

endef

ifeq ($(UNAME),Darwin)
$(eval $(call AndroidProvisioningTemplate,android-ndk-$(ANDROID_NDK_VERSION)-darwin-x86_64,ndk,,ndk))
$(eval $(call AndroidProvisioningTemplate,platform-tools_r$(ANDROID_PLATFORM_TOOLS_VERSION)-darwin,sdk,platform-tools))
$(eval $(call AndroidProvisioningTemplate,sdk-tools-darwin-$(ANDROID_SDKTOOLS_VERSION),sdk,tools))
$(eval $(call AndroidProvisioningTemplate,cmake-$(ANDROID_CMAKE_VERSION)-darwin-x86_64,sdk,cmake/$(ANDROID_CMAKE_VERSION)))
else ifeq ($(UNAME),Linux)
$(eval $(call AndroidProvisioningTemplate,android-ndk-$(ANDROID_NDK_VERSION)-linux-x86_64,ndk,,ndk))
$(eval $(call AndroidProvisioningTemplate,platform-tools_r$(ANDROID_PLATFORM_TOOLS_VERSION)-linux,sdk,platform-tools))
$(eval $(call AndroidProvisioningTemplate,sdk-tools-linux-$(ANDROID_SDKTOOLS_VERSION),sdk,tools))
$(eval $(call AndroidProvisioningTemplate,cmake-$(ANDROID_CMAKE_VERSION)-linux-x86_64,sdk,cmake/$(ANDROID_CMAKE_VERSION)))
else ifeq ($(UNAME),Windows)
$(eval $(call AndroidProvisioningTemplateStub,android-ndk-$(ANDROID_NDK_VERSION)-win32-x86_64,ndk,,ndk))
$(eval $(call AndroidProvisioningTemplateStub,platform-tools_r$(ANDROID_PLATFORM_TOOLS_VERSION)-win32,sdk,platform-tools))
$(eval $(call AndroidProvisioningTemplateStub,sdk-tools-win32-$(ANDROID_SDKTOOLS_VERSION),sdk,tools))
$(eval $(call AndroidProvisioningTemplateStub,cmake-$(ANDROID_CMAKE_VERSION)-win32-x86_64,sdk,cmake/$(ANDROID_CMAKE_VERSION)))
endif

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
_android-$(1)_CPP=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-$$(if $(wildcard $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-cpp),cpp,clang -E)
_android-$(1)_CXXCPP=$$(CCACHE) $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-$$(if $(wildcard $$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-cpp),cpp,clang++ -E)
_android-$(1)_DLLTOOL=
_android-$(1)_LD=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-ld
_android-$(1)_OBJDUMP="$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-objdump"
_android-$(1)_RANLIB=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-ranlib
_android-$(1)_CMAKE=$$(ANDROID_SDK_PREFIX)/cmake/$(ANDROID_CMAKE_VERSION)/bin/cmake
_android-$(1)_STRIP=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/bin/$(3)-strip

_android-$(1)_AC_VARS= \
	mono_cv_uscore=yes \
	ac_cv_func_sched_getaffinity=no \
	ac_cv_func_sched_setaffinity=no \
	ac_cv_func_shm_open_working_with_mmap=no

_android-$(1)_CFLAGS= \
	-fstack-protector \
	-DMONODROID=1 $$(if $$(filter $$(ANDROID_NEW_NDK),yes),-D__ANDROID_API__=$$(ANDROID_SDK_VERSION_$(1)))

_android-$(1)_CXXFLAGS= \
	-fstack-protector \
	-DMONODROID=1 $$(if $$(filter $$(ANDROID_NEW_NDK),yes),-D__ANDROID_API__=$$(ANDROID_SDK_VERSION_$(1)))


_android-$(1)_CPPFLAGS= \
	-I$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/sysroot/usr/include

_android-$(1)_CXXCPPFLAGS= \
	-I$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang/sysroot/usr/include

_android-$(1)_LDFLAGS= \
	-z now -z relro -z noexecstack \
	-ldl -lm -llog -lc -lgcc \
	-Wl,-rpath-link=$$(ANDROID_TOOLCHAIN_DIR)/ndk/platforms/android-$$(ANDROID_SDK_VERSION_$(1))/arch-$(2)/usr/lib,-dynamic-linker=/system/bin/linker \
	-L$$(ANDROID_TOOLCHAIN_DIR)/ndk/platforms/android-$$(ANDROID_SDK_VERSION_$(1))/arch-$(2)/usr/lib

_android-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-executables \
	--disable-iconv \
	--disable-mcs-build \
	--disable-nls \
	--enable-dynamic-btls \
	--enable-maintainer-mode \
	--enable-minimal=ssa,portability,attach,verifier,full_messages,sgen_remset,sgen_marksweep_par,sgen_marksweep_fixed,sgen_marksweep_fixed_par,sgen_copying,logging,security,shared_handles,interpreter,gac \
	--enable-monodroid \
	--with-btls-android-ndk=$$(ANDROID_TOOLCHAIN_DIR)/ndk \
	--with-btls-android-api=$$(ANDROID_SDK_VERSION_$(1)) \
	$$(if $$(filter $$(ANDROID_NEW_NDK),yes),--with-btls-android-ndk-asm-workaround) \
	--with-btls-android-cmake-toolchain=$$(ANDROID_TOOLCHAIN_DIR)/ndk/build/cmake/android.toolchain.cmake \
	--with-sigaltstack=yes \
	--with-tls=pthread \
	--without-ikvm-native \
	--disable-cooperative-suspend \
	--disable-hybrid-suspend \
	--disable-crash-reporting

.stamp-android-$(1)-toolchain: | $$(if $$(IGNORE_PROVISION_ANDROID),,provision-android)
	python "$$(ANDROID_TOOLCHAIN_DIR)/ndk/build/tools/make_standalone_toolchain.py" --verbose --force --api=$$(ANDROID_SDK_VERSION_$(1)) --arch=$(2) --install-dir=$$(ANDROID_TOOLCHAIN_PREFIX)/$(1)-clang
	touch $$@

$$(eval $$(call RuntimeTemplate,android,$(1),$(4)))

endef

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
define AndroidTargetTemplateStub

.stamp-android-$(1)-toolchain: | $$(if $$(IGNORE_PROVISION_ANDROID),,provision-android)
	touch $$@

$$(eval $$(call RuntimeTemplateStub,android,$(1),$(4)))

endef

## android-armeabi-v7a
android-armeabi-v7a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android-armeabi-v7a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -march=armv7-a -mtune=cortex-a8 -mfpu=vfp -mfloat-abi=softfp
android-armeabi-v7a_LDFLAGS=-Wl,--fix-cortex-a8
ifeq ($(UNAME),Windows)
$(eval $(call AndroidTargetTemplateStub,armeabi-v7a,arm,arm-linux-androideabi,armv5-linux-androideabi))
else
$(eval $(call AndroidTargetTemplate,armeabi-v7a,arm,arm-linux-androideabi,armv5-linux-androideabi))
endif

## android-arm64-v8a
android-arm64-v8a_CFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
android-arm64-v8a_CXXFLAGS=-D__POSIX_VISIBLE=201002 -DSK_RELEASE -DNDEBUG -UDEBUG -fpic -DL_cuserid=9 -DANDROID64
ifeq ($(UNAME),Windows)
$(eval $(call AndroidTargetTemplateStub,arm64-v8a,arm64,aarch64-linux-android,aarch64-linux-android))
else
$(eval $(call AndroidTargetTemplate,arm64-v8a,arm64,aarch64-linux-android,aarch64-linux-android))
endif

## android-x86
ifeq ($(UNAME),Windows)
$(eval $(call AndroidTargetTemplateStub,x86,x86,i686-linux-android,i686-linux-android))
else
$(eval $(call AndroidTargetTemplate,x86,x86,i686-linux-android,i686-linux-android))
endif

## android-x86_64
android-x86_64_CFLAGS=-DL_cuserid=9
android-x86_64_CXXFLAGS=-DL_cuserid=9
ifeq ($(UNAME),Windows)
$(eval $(call AndroidTargetTemplateStub,x86_64,x86_64,x86_64-linux-android,x86_64-linux-android))
else
$(eval $(call AndroidTargetTemplate,x86_64,x86_64,x86_64-linux-android,x86_64-linux-android))
endif

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

_android-$(1)_CFLAGS= \
	$$(if $$(filter $$(UNAME),Darwin),-mmacosx-version-min=10.9)

_android-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-iconv \
	--disable-mono-debugger \
	--disable-nls \
	--disable-mcs-build \
	--enable-dynamic-btls \
	--enable-maintainer-mode \
	--enable-monodroid \
	--with-mcs-docs=no \
	--without-ikvm-native \
	--with-static_mono=no \
	--disable-crash-reporting

.stamp-android-$(1)-toolchain:
	touch $$@

$$(eval $$(call RuntimeTemplate,android,$(1)))

ifeq ($$(UNAME),Darwin)
ADDITIONAL_PACKAGE_DEPS += $$(android_HOST_DARWIN_LIB_DIR)/.stamp-android-loader-path
endif

endef

# on Windows, we use the AndroidHostMxeTemplate, instead
ifneq ($(UNAME),Windows)
$(eval $(call AndroidHostTemplate,host-$(UNAME)))
endif

##
# Parameters
#  $(1): target
#  $(2): arch
define AndroidHostMxeTemplate

_android-$(1)_PATH=$$(MXE_PREFIX)/bin

_android-$(1)_AR=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-ar
_android-$(1)_AS=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-as
_android-$(1)_CC=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-gcc
_android-$(1)_CXX=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-g++
_android-$(1)_DLLTOOL=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-dlltool
_android-$(1)_LD=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-ld
_android-$(1)_OBJDUMP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-objdump
_android-$(1)_RANLIB=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-ranlib
_android-$(1)_STRIP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-strip

# On Cygwin, assume x86_64-mingw32-zlib and i686-mingw32-zlib are installed
# TODO: WSL packages will depend on the distro
_android-$(1)_AC_VARS= \
	$$(if $$(filter $$(UNAME),Windows),,ac_cv_header_zlib_h=no) \
	ac_cv_search_dlopen=no

_android-$(1)_CFLAGS= \
	-DXAMARIN_PRODUCT_VERSION=0 \
	$$(if $$(filter $$(UNAME),Windows),,-I$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/include)

_android-$(1)_CXXFLAGS= \
	-DXAMARIN_PRODUCT_VERSION=0 \
		$$(if $$(filter $$(UNAME),Windows),,-I$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/include)

_android-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-llvm \
	--disable-mcs-build \
	--disable-nls \
	--enable-static-gcc-libs \
	--enable-maintainer-mode \
	--enable-monodroid \
	--with-monodroid \
	--with-static_mono=no \
	--disable-crash-reporting

ifeq ($(UNAME),Darwin)
_android-$(1)_LDFLAGS= \
	$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/lib/libz.a
endif

.stamp-android-$(1)-toolchain:
	touch $$@

.stamp-android-$(1)-$$(CONFIGURATION)-configure: | $(if $(IGNORE_PROVISION_MXE),,provision-mxe)

$$(eval $$(call RuntimeTemplate,android,$(1),$(2)-w64-mingw32))

endef

ifneq ($(UNAME),Windows)
$(eval $(call AndroidHostMxeTemplate,host-mxe-Win32,i686))
$(eval $(call AndroidHostMxeTemplate,host-mxe-Win64,x86_64))
else
# on Windows the MinGW-built Mono is the host Mono.  But we have to use the cross template
# because 'gcc' is the cygwin or WSL compiler, while the x86_64-w64-mingw32-gcc is the windows native compiler.
# TODO: build $(eval $(call AndroidHostMxeTemplate,host-mxe-Win64,x86_64))
# TODO: also build $(eval $(call AndroidHostMxeTemplate,host-mxe-Win32,i686))
endif

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target (armeabi-v7a, arm64-v8a, x86 or x86_64)
#  $(5): llvm (llvm64 or llvmwin64)
#  $(6): offsets dumper abi
define AndroidCrossTemplate

_android-$(1)_OFFSETS_DUMPER_ARGS=--libclang="$$(ANDROID_LIBCLANG)" --sysroot="$$(ANDROID_TOOLCHAIN_DIR)/ndk/sysroot"

_android-$(1)_AR=ar
_android-$(1)_AS=as
_android-$(1)_CC=$$(or $$(android-$(1)_CC),cc)
_android-$(1)_CXX=$$(or $$(android-$(1)_CXX),c++)
_android-$(1)_CXXCPP=cpp
_android-$(1)_LD=ld
_android-$(1)_RANLIB=ranlib
_android-$(1)_STRIP=strip

_android-$(1)_CFLAGS= \
	$$(if $$(RELEASE),,-DDEBUG_CROSS) \
	$$(if $$(filter $$(UNAME),Darwin),-mmacosx-version-min=10.9) \
	-DXAMARIN_PRODUCT_VERSION=0

_android-$(1)_CXXFLAGS= \
	$$(if $$(RELEASE),,-DDEBUG_CROSS) \
	$$(if $$(filter $$(UNAME),Darwin),-mmacosx-version-min=10.9 -stdlib=libc++) \
	-DXAMARIN_PRODUCT_VERSION=0

_android-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-mcs-build \
	--disable-nls \
	--enable-maintainer-mode \
	--with-tls=pthread

$$(eval $$(call CrossRuntimeTemplate,android,$(1),$$(if $$(filter $$(UNAME),Darwin),$(2)-apple-darwin10,$$(if $$(filter $$(UNAME),Linux),$(2)-linux-gnu)),$(3)-linux-android,$(4),$(5),$(6)))

endef

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target (armeabi-v7a, arm64-v8a, x86 or x86_64)
#  $(5): llvm (llvm64 or llvmwin64)
#  $(6): offsets dumper abi
define AndroidCrossTemplateStub

$$(eval $$(call CrossRuntimeTemplateStub,android,$(1),$$(if $$(filter $$(UNAME),Darwin),$(2)-apple-darwin10,$$(if $$(filter $$(UNAME),Linux),$(2)-linux-gnu)),$(3)-linux-android,$(4),$(5),$(6)))

endef

ifeq ($(UNAME),Windows)
$(eval $(call AndroidCrossTemplateStub,cross-arm,x86_64,armv7,armeabi-v7a,llvm-llvm64,armv7-none-linux-androideabi))
$(eval $(call AndroidCrossTemplateStub,cross-arm64,x86_64,aarch64-v8a,arm64-v8a,llvm-llvm64,aarch64-v8a-linux-android))
$(eval $(call AndroidCrossTemplateStub,cross-x86,x86_64,i686,x86,llvm-llvm64,i686-none-linux-android))
$(eval $(call AndroidCrossTemplateStub,cross-x86_64,x86_64,x86_64,x86_64,llvm-llvm64,x86_64-none-linux-android))
else
$(eval $(call AndroidCrossTemplate,cross-arm,x86_64,armv7,armeabi-v7a,llvm-llvm64,armv7-none-linux-androideabi))
$(eval $(call AndroidCrossTemplate,cross-arm64,x86_64,aarch64-v8a,arm64-v8a,llvm-llvm64,aarch64-v8a-linux-android))
$(eval $(call AndroidCrossTemplate,cross-x86,x86_64,i686,x86,llvm-llvm64,i686-none-linux-android))
$(eval $(call AndroidCrossTemplate,cross-x86_64,x86_64,x86_64,x86_64,llvm-llvm64,x86_64-none-linux-android))
endif

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define AndroidCrossMXETemplate

_android-$(1)_OFFSETS_DUMPER_ARGS=--libclang="$$(ANDROID_LIBCLANG)" --sysroot="$$(ANDROID_TOOLCHAIN_DIR)/ndk/sysroot"

_android-$(1)_PATH=$$(MXE_PREFIX)/bin

_android-$(1)_AR=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-ar
_android-$(1)_AS=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-as
_android-$(1)_CC=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-gcc
_android-$(1)_CXX=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-g++
_android-$(1)_DLLTOOL=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-dlltool
_android-$(1)_LD=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-ld
_android-$(1)_OBJDUMP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-objdump
_android-$(1)_RANLIB=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-ranlib
_android-$(1)_STRIP=$$(MXE_PREFIX)/bin/$(2)-w64-mingw32-strip

_android-$(1)_CFLAGS= \
	$$(if $$(RELEASE),,-DDEBUG_CROSS) \
	-DXAMARIN_PRODUCT_VERSION=0 \
	-I$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/include

_android-$(1)_CXXFLAGS= \
	$$(if $$(RELEASE),,-DDEBUG_CROSS) \
	-DXAMARIN_PRODUCT_VERSION=0 \
	-I$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/include

_android-$(1)_LDFLAGS=


_android-$(1)_CONFIGURE_FLAGS= \
	--disable-boehm \
	--disable-mcs-build \
	--disable-nls \
	--enable-static-gcc-libs \
	--enable-maintainer-mode \
	--with-tls=pthread

ifeq ($(UNAME),Darwin)
_android-$(1)_CONFIGURE_FLAGS += \
	--with-static-zlib=$$(MXE_PREFIX)/opt/mingw-zlib/usr/$(2)-w64-mingw32/lib/libz.a
endif

.stamp-android-$(1)-$$(CONFIGURATION)-configure: | $(if $(IGNORE_PROVISION_MXE),,provision-mxe)

$$(eval $$(call CrossRuntimeTemplate,android,$(1),$(2)-w64-mingw32,$(3)-linux-android,$(4),$(5),$(6)))

endef

##
# Parameters
#  $(1): target
#  $(2): host arch
#  $(3): target arch
#  $(4): device target
#  $(5): llvm
#  $(6): offsets dumper abi
define AndroidCrossMXETemplateStub

$$(eval $$(call CrossRuntimeTemplateStub,android,$(1),$(2)-w64-mingw32,$(3)-linux-android,$(4),$(5),$(6)))

endef

ifneq ($(UNAME),Windows)
$(eval $(call AndroidCrossMXETemplate,cross-arm-win,x86_64,armv7,armeabi-v7a,llvm-llvmwin64,armv7-none-linux-androideabi))
$(eval $(call AndroidCrossMXETemplate,cross-arm64-win,x86_64,aarch64-v8a,arm64-v8a,llvm-llvmwin64,aarch64-v8a-linux-android))
$(eval $(call AndroidCrossMXETemplate,cross-x86-win,x86_64,i686,x86,llvm-llvmwin64,i686-none-linux-android))
$(eval $(call AndroidCrossMXETemplate,cross-x86_64-win,x86_64,x86_64,x86_64,llvm-llvmwin64,x86_64-none-linux-android))
else
$(eval $(call AndroidCrossMXETemplateStub,cross-arm-win,x86_64,armv7,armeabi-v7a,llvm-llvmwin64,armv7-none-linux-androideabi))
$(eval $(call AndroidCrossMXETemplateStub,cross-arm64-win,x86_64,aarch64-v8a,arm64-v8a,llvm-llvmwin64,aarch64-v8a-linux-android))
$(eval $(call AndroidCrossMXETemplateStub,cross-x86-win,x86_64,i686,x86,llvm-llvmwin64,i686-none-linux-android))
$(eval $(call AndroidCrossMXETemplateStub,cross-x86_64-win,x86_64,x86_64,x86_64,llvm-llvmwin64,x86_64-none-linux-android))
endif

ifeq ($(UNAME),Windows)
_bcl_android_BUILD_FLAGS += \
	PROFILE_PLATFORM=win32
endif

$(eval $(call BclTemplate,android,monodroid monodroid_tools,monodroid monodroid_tools))

$(android_SOURCES_DIR)/external/linker/README.md:  # we use this as a sentinel file to avoid rsyncing everything on each build (slows down iterating)
	mkdir -p $(android_SOURCES_DIR)/external/linker
	cd $(TOP) && rsync -r --include='*.cs' --include="README.md" --include="*/" --exclude="*" external/linker $(android_SOURCES_DIR)/external

$(android_SOURCES_DIR): $(android_SOURCES_DIR)/external/linker/README.md

$(android_TPN_DIR)/LICENSE:
	mkdir -p $(android_TPN_DIR)
	cd $(TOP) && rsync -r --include='THIRD-PARTY-NOTICES.TXT' --include='license.txt' --include='License.txt' --include='LICENSE' --include='LICENSE.txt' --include='LICENSE.TXT' --include='COPYRIGHT.regex' --include='*/' --exclude="*" --prune-empty-dirs . $(android_TPN_DIR)

$(android_TPN_DIR): $(android_TPN_DIR)/LICENSE

$(android_HOST_DARWIN_LIB_DIR)/.stamp-android-loader-path: package-android-host-Darwin
	$(android_PLATFORM_BIN)/install_name_tool -id @loader_path/libmonosgen-2.0.dylib $(android_HOST_DARWIN_LIB_DIR)/libmonosgen-2.0.dylib
	$(android_PLATFORM_BIN)/install_name_tool -change $(android_HOST_DARWIN_LIB_DIR)/libmonosgen-2.0.1.dylib @loader_path/libmonosgen-2.0.dylib $(android_HOST_DARWIN_BIN_DIR)/mono
	touch $@
