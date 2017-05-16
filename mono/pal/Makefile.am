
COREFX_ROOT=$(abs_top_srcdir)/external/corefx

.PHONY: $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native.a
$(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native.a:
	$(COREFX_ROOT)/src/Native/build-native.sh $(COREFX_PAL_ARCH) Release $(COREFX_PAL_OS) \
		cmakeargs -DCLR_DISABLE_SYSTEM_IO_COMPRESSION_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_NET_HTTP_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_NET_SECURITY_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_SECURITY_CRYPTOGRAPHY_NATIVE:BOOL=1 \
		cmakeargs -DCLR_DISABLE_SYSTEM_SECURITY_CRYPTOGRAPHY_NATIVE_APPLE:BOOL=1

System.Native.a: $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native.a
	cp $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native.a System.Native.a

all-local: System.Native.a

clean-local:
	-rm -rf System.Native.a

install-exec-local:
	$(mkinstalldirs) "$(DESTDIR)$(libdir)"
	$(install_sh) $(COREFX_ROOT)/bin/$(COREFX_PAL_OS).$(COREFX_PAL_ARCH).Release/native/System.Native.a "$(DESTDIR)$(libdir)"
