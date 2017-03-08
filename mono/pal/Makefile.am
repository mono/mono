
COREFX_ROOT=$(abs_top_srcdir)/external/corefx

all-local:
	$(COREFX_ROOT)/src/Native/build-native.sh $(COREFX_PAL_ARCH) Debug $(COREFX_PAL_OS) \
		cmakeargs -DCLR_DISABLE_SYSTEM_IO_COMPRESSION_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_NET_HTTP_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_NET_SECURITY_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_SECURITY_CRYPTOGRAPHY_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_SECURITY_CRYPTOGRAPHY_NATIVE_APPLE:BOOL=1
	cp $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Debug/native/System.Native$(libsuffix) System.Native$(libsuffix)

clean-local:
	-rm -rf System.Native$(libsuffix)

install-exec-local:
	$(mkinstalldirs) "$(DESTDIR)$(libdir)"
	$(install_sh) $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Debug/native/System.Native$(libsuffix) "$(DESTDIR)$(libdir)"
