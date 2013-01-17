thisdir = nunit24/NUnit.Framework/framework
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = NUnit.Framework.dll
LIBRARY_NAME = nunit.framework.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LIB_MCS_FLAGS = /r:System.Xml.dll /r:System.dll /d:StronglyNamedAssembly -warn:1
NO_TEST = yo

ifneq (net_2_0, $(PROFILE))
NO_INSTALL = yes
install-local: install-symlink
uninstall-local: uninstall-symlink
endif

EXTRA_DISTFILES = \
	nunit.framework.dll.csproj \
	nunit.framework.dll_VS2005.csproj

include ../../../build/library.make

symlinkdir = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
install-symlink:
	$(MKINSTALLDIRS) $(DESTDIR)$(symlinkdir)
	cd $(DESTDIR)$(symlinkdir) && rm -f $(LIBRARY_NAME) && ln -s ../2.0/$(LIBRARY_NAME) $(LIBRARY_NAME)

uninstall-symlink:
	rm -f $(DESTDIR)$(symlinkdir)/$(LIBRARY_NAME)
