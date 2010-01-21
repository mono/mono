thisdir = tools/xbuild
SUBDIRS = 
include ../../build/rules.make
HAS_NUNIT_TEST = yes

LOCAL_MCS_FLAGS = -r:Microsoft.Build.Framework.dll -r:Microsoft.Build.Utilities.dll -r:Microsoft.Build.Engine.dll
PROGRAM = xbuild.exe

include ../../build/executable.make

install-local:	install-extras

WEBAPP_DIR = Microsoft/VisualStudio/v9.0/WebApplications
EXTRAS_DIR = $(mono_libdir)/mono/2.0
install-extras: 
	$(MKINSTALLDIRS) $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/xbuild.rsp $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.Common.tasks $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.Build.xsd $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.Common.targets $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.CSharp.targets $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.VisualBasic.targets $(DESTDIR)$(EXTRAS_DIR)
	$(MKINSTALLDIRS) $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.CommonTypes.xsd $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.Core.xsd $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.WebApplication.targets $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)

EXTRA_DISTFILES = \
	xbuild/xbuild.rsp \
	xbuild/Microsoft.Common.tasks \
	xbuild/Microsoft.Build.xsd \
	xbuild/Microsoft.CSharp.targets \
	xbuild/Microsoft.Common.targets \
	xbuild/MSBuild/Microsoft.Build.CommonTypes.xsd \
	xbuild/MSBuild/Microsoft.Build.Core.xsd		\
	xbuild/Microsoft.VisualBasic.targets \
	xbuild/Microsoft.WebApplication.targets
	
	

