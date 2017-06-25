CORERT_ROOT=$(abs_top_srcdir)/external/corert

all-local:
	$(CORERT_ROOT)/buildscripts/build-native.sh $(CORERT_PAL_ARCH) Release
	cp $(CORERT_ROOT)/bin/Product/$(CORERT_PAL_OS).$(CORERT_PAL_ARCH).Release/lib/System.Private.CoreLib.Native-Dynamic$(libsuffix)$  \
	System.Native.CoreRT$(libsuffix)$

clean-local:
	-rm -rf System.Native.CoreRT$(libsuffix)$
	-rm -rf $(CORERT_ROOT)/bin/Product/$(CORERT_PAL_OS).$(CORERT_PAL_ARCH).Release

install-exec-local:
	$(mkinstalldirs) "$(DESTDIR)$(libdir)"
	$(install_sh) $(CORERT_ROOT)/bin/Product/$(CORERT_PAL_OS).$(CORERT_PAL_ARCH).Release/lib/System.Private.CoreLib.Native-Dynamic$(libsuffix)$  \
	"$(DESTDIR)$(libdir)/System.Native.CoreRT$(libsuffix)"
