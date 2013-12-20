thisdir = tools/xbuild
SUBDIRS = 
include ../../build/rules.make
NO_TESTS = yes

include xbuild.make

LOCAL_MCS_FLAGS = -r:$(XBUILD_FRAMEWORK) -r:$(XBUILD_UTILITIES) -r:$(XBUILD_ENGINE)
PROGRAM = xbuild.exe

include ../../build/executable.make

XBUILD_DIR=.
XBUILD_FRAMEWORKS_DIR=$(mono_libdir)/mono/xbuild-frameworks/.NETFramework/
REDISTLIST_DIR=$(XBUILD_FRAMEWORKS_DIR)/v$(FRAMEWORK_VERSION)/RedistList
include $(XBUILD_DIR)/xbuild_targets.make

install-local:	install-extras

WEBAPP_DIR = Microsoft/VisualStudio
SILVERLIGHT_DIR = $(mono_libdir)/mono/xbuild/Microsoft/Silverlight
PORTABLE_DIR = $(mono_libdir)/mono/xbuild/Microsoft/Portable

install-extras: 
	$(MKINSTALLDIRS) $(DESTDIR)$(XBUILD_BIN_DIR) $(DESTDIR)$(REDISTLIST_DIR) $(DESTDIR)$(XBUILD_FRAMEWORKS_DIR)/v3.0/RedistList $(DESTDIR)$(XBUILD_FRAMEWORKS_DIR)/v4.0/RedistList $(DESTDIR)$(mono_libdir)/mono/3.5 $(DESTDIR)$(mono_libdir)/mono/4.0
	$(INSTALL_DATA) xbuild/xbuild.rsp $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) xbuild/$(XBUILD_VERSION)/Microsoft.Common.tasks $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) xbuild/$(XBUILD_VERSION)/Microsoft.Common.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.Build.xsd $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.CSharp.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.VisualBasic.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) xbuild/$(FRAMEWORK_VERSION)/FrameworkList.xml $(DESTDIR)$(REDISTLIST_DIR)
	$(INSTALL_DATA) xbuild/4.0/FrameworkList.xml $(DESTDIR)$(XBUILD_FRAMEWORKS_DIR)/v4.0/RedistList/FrameworkList.xml
	$(INSTALL_DATA) xbuild/FrameworkList-3.0.xml $(DESTDIR)$(XBUILD_FRAMEWORKS_DIR)/v3.0/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.CommonTypes.xsd $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.Core.xsd $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)/v9.0/WebApplications
	$(INSTALL_DATA) xbuild/Microsoft.WebApplication.targets $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)/v9.0/WebApplications
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)/v10.0/WebApplications
	$(INSTALL_DATA) xbuild/Microsoft.WebApplication.targets $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)/v10.0/WebApplications
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)/v11.0/WebApplications
	$(INSTALL_DATA) xbuild/Microsoft.WebApplication.targets $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)/v11.0/WebApplications
	$(MKINSTALLDIRS) $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0
	$(MKINSTALLDIRS) $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.CSharp.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.CSharp.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.VisualBasic.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.VisualBasic.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0
	sed -e 's/@SILVERLIGHT_VERSION@/2.0/g' xbuild/Microsoft.Silverlight.Common.targets > $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0/Microsoft.Silverlight.Common.targets
	sed -e 's/@SILVERLIGHT_VERSION@/3.0/g' xbuild/Microsoft.Silverlight.Common.targets > $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0/Microsoft.Silverlight.Common.targets
	$(MKINSTALLDIRS) $(DESTDIR)$(PORTABLE_DIR)/v4.0
	$(MKINSTALLDIRS) $(DESTDIR)$(PORTABLE_DIR)/v4.5
	$(INSTALL_DATA) xbuild/4.0/Microsoft.Portable.CSharp.targets $(DESTDIR)$(PORTABLE_DIR)/v4.0
	$(INSTALL_DATA) xbuild/4.5/Microsoft.Portable.CSharp.targets $(DESTDIR)$(PORTABLE_DIR)/v4.5
	$(INSTALL_DATA) xbuild/Microsoft.Portable.CSharp.targets $(DESTDIR)$(mono_libdir)/mono/4.0

EXTRA_DISTFILES = \
	xbuild/xbuild.rsp \
	xbuild/2.0/Microsoft.Common.tasks \
	xbuild/3.5/Microsoft.Common.tasks \
	xbuild/4.0/Microsoft.Common.tasks \
	xbuild/2.0/Microsoft.Common.targets \
	xbuild/3.5/Microsoft.Common.targets \
	xbuild/4.0/Microsoft.Common.targets \
	xbuild/4.0/Microsoft.Portable.CSharp.targets \
	xbuild/4.5/Microsoft.Portable.CSharp.targets \
	xbuild/2.0/FrameworkList.xml \
	xbuild/3.5/FrameworkList.xml \
	xbuild/4.0/FrameworkList.xml \
	xbuild/4.5/FrameworkList.xml \
	xbuild/FrameworkList-3.0.xml \
	xbuild/Microsoft.Build.xsd \
	xbuild/Microsoft.CSharp.targets \
	xbuild/MSBuild/Microsoft.Build.CommonTypes.xsd \
	xbuild/MSBuild/Microsoft.Build.Core.xsd		\
	xbuild/Microsoft.VisualBasic.targets \
	xbuild/Microsoft.WebApplication.targets \
	xbuild/Microsoft.Silverlight.Common.targets \
	xbuild/Microsoft.Silverlight.CSharp.targets \
	xbuild/Microsoft.Silverlight.VisualBasic.targets \
	xbuild/Microsoft.Portable.CSharp.targets \
	xbuild_targets.make
