thisdir = nunit20/NUnitCore/core
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= -r:nunit.framework.dll -r:nunit.core.interfaces.dll -r:System.dll /d:StronglyNamedAssembly -warn:1
NO_TEST = yo

ifneq (net_2_0, $(PROFILE))
NO_INSTALL = yes
install-local: install-symlink
uninstall-local: uninstall-symlink
endif

EXTRA_DISTFILES = nunit.core.dll.csproj nunit.core.dll_VS2005.csproj

include ../../../build/library.make

symlinkdir = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
install-symlink:
	$(MKINSTALLDIRS) $(DESTDIR)$(symlinkdir)
	cd $(DESTDIR)$(symlinkdir) && rm -f $(LIBRARY_NAME) && ln -s ../2.0/$(LIBRARY_NAME) $(LIBRARY_NAME)

uninstall-symlink:
	rm -f $(DESTDIR)$(symlinkdir)/$(LIBRARY_NAME)
