thisdir = nunit24/NUnitExtensions/core
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit.core.extensions.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LIB_MCS_FLAGS = \
	-debug \
	/r:nunit.core.dll /r:nunit.core.interfaces.dll \
	/r:System.Xml.dll /r:System.dll /d:StronglyNamedAssembly
NO_TEST = yo

ifeq (net_2_0, $(PROFILE))
NO_INSTALL = yes
install-local: install-symlink
uninstall-local: uninstall-symlink
endif

EXTRA_DISTFILES = \
	nunit.core.extensions.dll.csproj \
	nunit.core.extensions.dll_VS2005.csproj

include ../../../build/library.make

symlinkdir = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
install-symlink:
	$(MKINSTALLDIRS) $(DESTDIR)$(symlinkdir)
	cd $(DESTDIR)$(symlinkdir) && rm -f $(LIBRARY_NAME) && ln -s ../1.0/$(LIBRARY_NAME) $(LIBRARY_NAME)

uninstall-symlink:
	rm -f $(DESTDIR)$(symlinkdir)/$(LIBRARY_NAME)
