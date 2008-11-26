thisdir = nunit24/NUnitCore/interfaces
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.interfaces.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= -debug -r:nunit.framework.dll -r:System.dll /d:StronglyNamedAssembly
NO_TEST = yo

ifeq (net_2_0, $(PROFILE))
NO_INSTALL = yes
install-local: install-symlink
uninstall-local: uninstall-symlink
endif

EXTRA_DISTFILES = nunit.core.interfaces.dll.csproj nunit.core.interfaces.dll_VS2005.csproj

include ../../../build/library.make

symlinkdir = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
install-symlink:
	$(MKINSTALLDIRS) $(DESTDIR)$(symlinkdir)
	cd $(DESTDIR)$(symlinkdir) && rm -f $(LIBRARY_NAME) && ln -s ../1.0/$(LIBRARY_NAME) $(LIBRARY_NAME)

uninstall-symlink:
	rm -f $(DESTDIR)$(symlinkdir)/$(LIBRARY_NAME)
