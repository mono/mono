thisdir = tools/xbuild
SUBDIRS = 
include ../../build/rules.make

LOCAL_MCS_FLAGS = -r:Microsoft.Build.Framework.dll -r:Microsoft.Build.Utilities.dll -r:Microsoft.Build.Engine.dll
PROGRAM = xbuild.exe

include ../../build/executable.make

install-local:	install-extras

EXTRAS_DIR = $(mono_libdir)/mono/xbuild
install-extras: 
	$(MKINSTALLDIRS) $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/xbuild.rsp $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.Common.tasks $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.Build.xsd $(DESTDIR)$(EXTRAS_DIR)

	$(MKINSTALLDIRS) $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.CommonTypes.xsd $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.Core.xsd $(DESTDIR)$(EXTRAS_DIR)/MSBuild

EXTRA_DISTFILES = \
	xbuild/xbuild.rsp \
	xbuild/Microsoft.Common.tasks \
	xbuild/Microsoft.Build.xsd \
	xbuild/MSBuild/Microsoft.Build.CommonTypes.xsd \
	xbuild/MSBuild/Microsoft.Build.Core.xsd
