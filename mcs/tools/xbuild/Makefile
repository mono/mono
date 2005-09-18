thisdir = tools/xbuild
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Microsoft.Build.Framework.dll -r:Microsoft.Build.Utilities.dll -r:Microsoft.Build.Engine.dll
PROGRAM = xbuild.exe

include ../../build/executable.make

# FIXME: it causes warnings, maybe only part of executable.make should be included?

install-local:
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib/mono/1.0
	$(INSTALL_DATA) xbuild.rsp $(DESTDIR)$(prefix)/lib/mono/1.0
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib/mono/1.0
	$(INSTALL_DATA) Microsoft.Build.xsd $(DESTDIR)$(prefix)/lib/mono/1.0
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib/mono/1.0
	$(INSTALL_DATA) Microsoft.Common.tasks $(DESTDIR)$(prefix)/lib/mono/1.0
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib/mono/1.0/MSBuild
	$(INSTALL_DATA) MSBuild/Microsoft.Build.Commontypes.xsd $(DESTDIR)$(prefix)/lib/mono/1.0/MSBuild
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib/mono/1.0/MSBuild
	$(INSTALL_DATA) MSBuild/Microsoft.Build.Core.xsd $(DESTDIR)$(prefix)/lib/mono/1.0/MSBuild
