thisdir = nunit24/ConsoleRunner/nunit-console
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = nunit-console-runner.dll
LIBRARY_SNK = $(topdir)/nunit24/nunit.snk

LOCAL_MCS_FLAGS= \
	-r:nunit.core.dll -r:nunit.core.interfaces.dll -r:nunit.util.dll \
	-r:System.dll -r:System.Xml.dll \
	/d:MONO /d:StronglyNamedAssembly
NO_TEST = yo

EXTRA_DISTFILES = nunit-console.csproj nunit-console_VS2005.csproj

ifeq (net_2_0, $(PROFILE))
NO_INSTALL = yes
install-local: install-symlink
uninstall-local: uninstall-symlink
endif

include ../../../build/library.make

symlinkdir = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
install-symlink:
	$(MKINSTALLDIRS) $(DESTDIR)$(symlinkdir)
	cd $(DESTDIR)$(symlinkdir) && rm -f $(LIBRARY_NAME) && ln -s ../1.0/$(LIBRARY_NAME) $(LIBRARY_NAME)

uninstall-symlink:
	rm -f $(DESTDIR)$(symlinkdir)/$(LIBRARY_NAME)
