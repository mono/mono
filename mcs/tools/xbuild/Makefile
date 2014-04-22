thisdir = tools/xbuild
SUBDIRS = 
include ../../build/rules.make
NO_TESTS = yes

include xbuild.make

LOCAL_MCS_FLAGS = -r:$(XBUILD_FRAMEWORK) -r:$(XBUILD_UTILITIES) -r:$(XBUILD_ENGINE)
PROGRAM = xbuild.exe

include ../../build/executable.make

XBUILD_DIR=.
include $(XBUILD_DIR)/xbuild_test.make

ifeq (4.0, $(FRAMEWORK_VERSION))
install-local: xbuild-net4-fail
else
install-local: install-extras
endif

NETFRAMEWORK_DIR=$(mono_libdir)/mono/xbuild-frameworks/.NETFramework
VS_TARGETS_DIR = $(mono_libdir)/mono/xbuild/Microsoft/VisualStudio
PORTABLE_TARGETS_DIR = $(mono_libdir)/mono/xbuild/Microsoft/Portable


install-extras: install-bin-data install-frameworks install-pcl-targets install-web-targets

install-bin-data:
	$(MKINSTALLDIRS) $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	$(INSTALL_DATA) data/xbuild.rsp $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/$(XBUILD_VERSION)/Microsoft.Common.tasks $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/$(XBUILD_VERSION)/Microsoft.Common.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/$(XBUILD_VERSION)/Microsoft.CSharp.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/Microsoft.Build.xsd $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/Microsoft.VisualBasic.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/MSBuild/Microsoft.Build.CommonTypes.xsd $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	$(INSTALL_DATA) data/MSBuild/Microsoft.Build.Core.xsd $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	sed -e 's/@ASM_VERSION@/$(XBUILD_ASSEMBLY_VERSION)/g' data/xbuild.exe.config.in > $(DESTDIR)$(XBUILD_BIN_DIR)/xbuild.exe.config

install-frameworks:
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v$(FRAMEWORK_VERSION)/RedistList
	$(INSTALL_DATA) frameworks/net_$(FRAMEWORK_VERSION).xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v$(FRAMEWORK_VERSION)/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v3.0/RedistList
	$(INSTALL_DATA) frameworks/net_3.0.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v3.0/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/RedistList
	$(INSTALL_DATA) frameworks/net_4.0.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/Profile/Client/RedistList
	$(INSTALL_DATA) frameworks/net_4.0_client.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/Profile/Client/RedistList/FrameworkList.xml

install-pcl-targets:
	$(MKINSTALLDIRS) $(DESTDIR)$(PORTABLE_TARGETS_DIR)/v4.0
	$(INSTALL_DATA) targets/Microsoft.Portable.Common.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/v4.0/Microsoft.Portable.Common.targets
	$(INSTALL_DATA) targets/Microsoft.Portable.CSharp_4.0.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/v4.0/Microsoft.Portable.CSharp.targets
	$(MKINSTALLDIRS) $(DESTDIR)$(PORTABLE_TARGETS_DIR)/v4.5
	$(INSTALL_DATA) targets/Microsoft.Portable.Common.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/v4.5/Microsoft.Portable.Common.targets
	$(INSTALL_DATA) targets/Microsoft.Portable.CSharp_4.5.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/v4.5/Microsoft.Portable.CSharp.targets
	$(INSTALL_DATA) targets/Microsoft.Portable.Core.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/Microsoft.Portable.Core.targets
	$(INSTALL_DATA) targets/Microsoft.Portable.Core.props $(DESTDIR)$(PORTABLE_TARGETS_DIR)/Microsoft.Portable.Core.props

install-web-targets:
	$(MKINSTALLDIRS) $(DESTDIR)$(VS_TARGETS_DIR)/v9.0/WebApplications
	$(INSTALL_DATA) targets/Microsoft.WebApplication.targets $(DESTDIR)$(VS_TARGETS_DIR)/v9.0/WebApplications
	$(MKINSTALLDIRS) $(DESTDIR)$(VS_TARGETS_DIR)/v10.0/WebApplications
	$(INSTALL_DATA) targets/Microsoft.WebApplication.targets $(DESTDIR)$(VS_TARGETS_DIR)/v10.0/WebApplications
	$(MKINSTALLDIRS) $(DESTDIR)$(VS_TARGETS_DIR)/v11.0/WebApplications
	$(INSTALL_DATA) targets/Microsoft.WebApplication.targets $(DESTDIR)$(VS_TARGETS_DIR)/v11.0/WebApplications

EXTRA_DISTFILES = \
	data/xbuild.rsp \
	data/xbuild.exe.config.in \
	data/Microsoft.Build.xsd \
	data/2.0/Microsoft.Common.tasks \
	data/3.5/Microsoft.Common.tasks \
	data/4.0/Microsoft.Common.tasks \
	data/12.0/Microsoft.Common.tasks \
	data/2.0/Microsoft.Common.targets \
	data/3.5/Microsoft.Common.targets \
	data/4.0/Microsoft.Common.targets \
	data/12.0/Microsoft.Common.targets \
	data/2.0/Microsoft.CSharp.targets \
	data/3.5/Microsoft.CSharp.targets \
	data/4.0/Microsoft.CSharp.targets \
	data/12.0/Microsoft.CSharp.targets \
	data/Microsoft.VisualBasic.targets \
	data/MSBuild/Microsoft.Build.CommonTypes.xsd \
	data/MSBuild/Microsoft.Build.Core.xsd \
	frameworks/net_2.0.xml \
	frameworks/net_3.0.xml \
	frameworks/net_3.5.xml \
	frameworks/net_4.0.xml \
	frameworks/net_4.0_client.xml \
	frameworks/net_4.5.xml \
	targets/Microsoft.Portable.CSharp_4.0.targets \
	targets/Microsoft.Portable.CSharp_4.5.targets \
	targets/Microsoft.Portable.Common.targets \
	targets/Microsoft.Portable.Core.targets \
	targets/Microsoft.Portable.Core.props \
	targets/Microsoft.WebApplication.targets \
	xbuild.make \
	xbuild_test.make
