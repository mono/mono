thisdir = tools/xbuild
SUBDIRS = 
include ../../build/rules.make
HAS_NUNIT_TEST = yes

BUILD_FRAMEWORK = Microsoft.Build.Framework.dll
BUILD_ENGINE = Microsoft.Build.Engine.dll

ifeq (3.5, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v3.5
ASSEMBLY_VERSION = 3.5.0.0
BUILD_FRAMEWORK := $(topdir)/class/lib/$(PROFILE)/$(BUILD_FRAMEWORK)
BUILD_ENGINE := $(topdir)/class/lib/$(PROFILE)/$(BUILD_ENGINE)
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
NAME_SUFFIX = .v4.0
ASSEMBLY_VERSION = 4.0.0.0
endif
endif

LOCAL_MCS_FLAGS = -r:$(BUILD_FRAMEWORK) -r:Microsoft.Build.Utilities$(NAME_SUFFIX).dll -r:$(BUILD_ENGINE)
PROGRAM = $(topdir)/class/lib/$(PROFILE)/xbuild.exe

include ../../build/executable.make

CLEAN_FILES= xbuild.exe xbuild.exe.mdb

XBUILD_DIR=.
XBUILD_FRAMEWORKS_DIR=$(mono_libdir)/mono/xbuild-frameworks/.NETFramework/
REDISTLIST_DIR=$(XBUILD_FRAMEWORKS_DIR)/v$(FRAMEWORK_VERSION)/RedistList
include $(XBUILD_DIR)/xbuild_targets.make

install-local:	install-extras

WEBAPP_DIR = Microsoft/VisualStudio/v9.0/WebApplications
SILVERLIGHT_DIR = $(mono_libdir)/mono/xbuild/Microsoft/Silverlight
EXTRAS_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)
install-extras: 
	$(MKINSTALLDIRS) $(DESTDIR)$(EXTRAS_DIR) $(DESTDIR)$(REDISTLIST_DIR) $(DESTDIR)$(XBUILD_FRAMEWORKS_DIR)/v3.0/RedistList
	$(INSTALL_DATA) xbuild/xbuild.rsp $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/$(FRAMEWORK_VERSION)/Microsoft.Common.tasks $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.Build.xsd $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/$(FRAMEWORK_VERSION)/Microsoft.Common.targets $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.CSharp.targets $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.VisualBasic.targets $(DESTDIR)$(EXTRAS_DIR)
	$(INSTALL_DATA) xbuild/$(FRAMEWORK_VERSION)/FrameworkList.xml $(DESTDIR)$(REDISTLIST_DIR)
	$(INSTALL_DATA) xbuild/FrameworkList-3.0.xml $(DESTDIR)$(XBUILD_FRAMEWORKS_DIR)/v3.0/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.CommonTypes.xsd $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(INSTALL_DATA) xbuild/MSBuild/Microsoft.Build.Core.xsd $(DESTDIR)$(EXTRAS_DIR)/MSBuild
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)
	$(INSTALL_DATA) xbuild/Microsoft.WebApplication.targets $(DESTDIR)$(mono_libdir)/mono/xbuild/$(WEBAPP_DIR)
	$(MKINSTALLDIRS) $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0
	$(MKINSTALLDIRS) $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.CSharp.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.CSharp.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.VisualBasic.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0
	$(INSTALL_DATA) xbuild/Microsoft.Silverlight.VisualBasic.targets $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0
	sed -e 's/@SILVERLIGHT_VERSION@/2.0/g' xbuild/Microsoft.Silverlight.Common.targets > $(DESTDIR)$(SILVERLIGHT_DIR)/v2.0/Microsoft.Silverlight.Common.targets
	sed -e 's/@SILVERLIGHT_VERSION@/3.0/g' xbuild/Microsoft.Silverlight.Common.targets > $(DESTDIR)$(SILVERLIGHT_DIR)/v3.0/Microsoft.Silverlight.Common.targets

EXTRA_DISTFILES = \
	xbuild/xbuild.rsp \
	xbuild/2.0/Microsoft.Common.tasks \
	xbuild/3.5/Microsoft.Common.tasks \
	xbuild/4.0/Microsoft.Common.tasks \
	xbuild/2.0/Microsoft.Common.targets \
	xbuild/3.5/Microsoft.Common.targets \
	xbuild/4.0/Microsoft.Common.targets \
	xbuild/2.0/FrameworkList.xml \
	xbuild/3.5/FrameworkList.xml \
	xbuild/4.0/FrameworkList.xml \
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
	xbuild_targets.make
