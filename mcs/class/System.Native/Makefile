thisdir = class/System.Native
SUBDIRS =
include ../../build/rules.make

ifeq ($(HOST_PLATFORM),darwin)
LIBSUFFIX = .dylib
else
ifeq ($(HOST_PLATFORM),win32)
LIBSUFFIX = .dll
else
ifeq ($(HOST_PLATFORM),linux)
LIBSUFFIX = .so
else
DISABLE_COREFX_PAL = yes
endif
endif
endif

ifndef DISABLE_COREFX_PAL

COREFX_ROOT=../../../external/corefx

.PHONY: $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native$(LIBSUFFIX)
$(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native$(LIBSUFFIX):
	$(COREFX_ROOT)/src/Native/build-native.sh $(COREFX_PAL_ARCH) Release $(COREFX_PAL_OS) \
		cmakeargs -DCLR_DISABLE_SYSTEM_IO_COMPRESSION_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_NET_HTTP_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_NET_SECURITY_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_SECURITY_CRYPTOGRAPHY_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_SECURITY_CRYPTOGRAPHY_NATIVE_APPLE:BOOL=1

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Native$(LIBSUFFIX): $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native$(LIBSUFFIX)
	cp $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native$(LIBSUFFIX) $(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Native$(LIBSUFFIX)

all-local: $(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Native$(LIBSUFFIX)

clean-local:
	-rm -rf $(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Native$(LIBSUFFIX)
	-rm -rf $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release

install-exec-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(LIBRARY_INSTALL_DIR)
	$(INSTALL_LIB) $(topdir)/class/lib/$(PROFILE_DIRECTORY)/System.Native$(LIBSUFFIX) $(DESTDIR)$(LIBRARY_INSTALL_DIR)/System.Native$(LIBSUFFIX)

endif

csproj-local:
