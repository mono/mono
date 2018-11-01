thisdir = tools/xbuild
SUBDIRS = 
include ../../build/rules.make
NO_TESTS = yes

include xbuild.make

LOCAL_MCS_FLAGS =
LIB_REFS = $(XBUILD_FRAMEWORK) $(XBUILD_UTILITIES) $(XBUILD_ENGINE) $(XBUILD_MSTASKS) $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core
PROGRAM = xbuild.exe

include ../../build/executable.make

XBUILD_DIR=.
include $(XBUILD_DIR)/xbuild_test.make

install-local: install-extras

PORTABLE_TARGETS_SRC:=data/Portable/Targets
PCL5_FX_SRC:=data/Portable/Frameworks/v5.0
DENIED_ASSEMBLY_LIST_SRC=data/deniedAssembliesList.txt

NETFRAMEWORK_DIR=$(mono_libdir)/mono/xbuild-frameworks/.NETFramework
PCL5_FX_DIR=$(mono_libdir)/mono/xbuild-frameworks/.NETPortable/v5.0
VS_TARGETS_DIR = $(mono_libdir)/mono/xbuild/Microsoft/VisualStudio
PORTABLE_TARGETS_DIR = $(mono_libdir)/mono/xbuild/Microsoft/Portable
NUGET_BUILDTASKS_TARGETS_DIR = $(mono_libdir)/mono/xbuild/Microsoft/NuGet
MSBUILD_EXTENSIONS_15_DIR=$(mono_libdir)/mono/xbuild/15.0

ifeq (14.0, $(XBUILD_VERSION))
install-extras: install-versioned-files install-global-files
else
install-extras: install-versioned-files
endif

#install files into xbuild's versioned locations
install-versioned-files: install-bin-data install-nuget-imports

#install files that are only installed once across all xbuild versions
install-global-files: install-frameworks install-web-targets install-pcl-targets install-pcl5-framework install-nuget-targets install-msbuild-specific-files

install-bin-data:
	$(MKINSTALLDIRS) $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	$(INSTALL_DATA) data/xbuild.rsp $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/$(XBUILD_VERSION)/Microsoft.Common.tasks $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/$(XBUILD_VERSION)/Microsoft.Common.targets $(DESTDIR)$(XBUILD_BIN_DIR)
ifeq (14.0, $(XBUILD_VERSION))
	$(INSTALL_DATA) data/$(XBUILD_VERSION)/Microsoft.Common.props $(DESTDIR)$(XBUILD_BIN_DIR)/../
endif
	$(INSTALL_DATA) data/$(XBUILD_VERSION)/Microsoft.CSharp.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/Microsoft.Build.xsd $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/Microsoft.VisualBasic.targets $(DESTDIR)$(XBUILD_BIN_DIR)
	$(INSTALL_DATA) data/MSBuild/Microsoft.Build.CommonTypes.xsd $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	$(INSTALL_DATA) data/MSBuild/Microsoft.Build.Core.xsd $(DESTDIR)$(XBUILD_BIN_DIR)/MSBuild
	sed -e 's/@ASM_VERSION@/$(XBUILD_ASSEMBLY_VERSION)/g' data/xbuild.exe.config.in > $(DESTDIR)$(XBUILD_BIN_DIR)/xbuild.exe.config

install-frameworks:
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v$(FRAMEWORK_VERSION)/RedistList
	$(INSTALL_DATA) frameworks/net_$(FRAMEWORK_VERSION).xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v$(FRAMEWORK_VERSION)/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v2.0/RedistList
	$(INSTALL_DATA) frameworks/net_2.0.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v2.0/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v3.0/RedistList
	$(INSTALL_DATA) frameworks/net_3.0.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v3.0/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v3.5/RedistList
	$(INSTALL_DATA) frameworks/net_3.5.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v3.5/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/RedistList
	$(INSTALL_DATA) frameworks/net_4.0.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/Profile/Client/RedistList
	$(INSTALL_DATA) frameworks/net_4.0_client.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.0/Profile/Client/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.5.1/RedistList
	$(INSTALL_DATA) frameworks/net_4.5.1.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.5.1/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.5.2/RedistList
	$(INSTALL_DATA) frameworks/net_4.5.2.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.5.2/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.6/RedistList
	$(INSTALL_DATA) frameworks/net_4.6.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.6/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.6.1/RedistList
	$(INSTALL_DATA) frameworks/net_4.6.1.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.6.1/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.6.2/RedistList
	$(INSTALL_DATA) frameworks/net_4.6.2.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.6.2/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.7/RedistList
	$(INSTALL_DATA) frameworks/net_4.7.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.7/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.7.1/RedistList
	$(INSTALL_DATA) frameworks/net_4.7.1.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.7.1/RedistList/FrameworkList.xml
	$(MKINSTALLDIRS) $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.7.2/RedistList
	$(INSTALL_DATA) frameworks/net_4.7.2.xml $(DESTDIR)$(NETFRAMEWORK_DIR)/v4.7.2/RedistList/FrameworkList.xml

install-pcl-targets:
	$(MKINSTALLDIRS) $(DESTDIR)$(PORTABLE_TARGETS_DIR)
	$(INSTALL_DATA) $(PORTABLE_TARGETS_SRC)/Microsoft.Portable.Core.props $(DESTDIR)$(PORTABLE_TARGETS_DIR)/Microsoft.Portable.Core.props
	$(INSTALL_DATA) $(PORTABLE_TARGETS_SRC)/Microsoft.Portable.Core.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/Microsoft.Portable.Core.targets

	for VERSION in v4.0 v4.5 v4.6 v5.0; do \
		$(MKINSTALLDIRS) $(DESTDIR)$(PORTABLE_TARGETS_DIR)/$$VERSION; \
		$(INSTALL_DATA) $(PORTABLE_TARGETS_SRC)/$$VERSION/Microsoft.Portable.Common.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/$$VERSION/Microsoft.Portable.Common.targets; \
		$(INSTALL_DATA) $(PORTABLE_TARGETS_SRC)/$$VERSION/Microsoft.Portable.CSharp.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/$$VERSION/Microsoft.Portable.CSharp.targets; \
		$(INSTALL_DATA) $(PORTABLE_TARGETS_SRC)/$$VERSION/Microsoft.Portable.VisualBasic.targets $(DESTDIR)$(PORTABLE_TARGETS_DIR)/$$VERSION/Microsoft.Portable.VisualBasic.targets; \
	done

install-web-targets:
	for VERSION in v9.0 v10.0 v11.0 v12.0 v14.0 v15.0; do \
		$(MKINSTALLDIRS) $(DESTDIR)$(VS_TARGETS_DIR)/$$VERSION/WebApplications; \
		$(INSTALL_DATA) targets/Microsoft.WebApplication.targets $(DESTDIR)$(VS_TARGETS_DIR)/$$VERSION/WebApplications; \
	done

NUGET_BUILDTASKS_REPO_DIR=$(topdir)/../external/nuget-buildtasks

install-nuget-targets:
	$(MKINSTALLDIRS) $(DESTDIR)$(NUGET_BUILDTASKS_TARGETS_DIR)
	$(MKINSTALLDIRS) $(DESTDIR)$(MSBUILD_EXTENSIONS_15_DIR)/Imports/Microsoft.Common.props/ImportBefore
	$(MKINSTALLDIRS) $(DESTDIR)$(MSBUILD_EXTENSIONS_15_DIR)/Microsoft.Common.targets/ImportAfter
	$(INSTALL_DATA) $(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/Microsoft.NuGet.targets $(DESTDIR)$(NUGET_BUILDTASKS_TARGETS_DIR)
	$(INSTALL_DATA) $(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/Microsoft.NuGet.props $(DESTDIR)$(NUGET_BUILDTASKS_TARGETS_DIR)
	$(INSTALL_DATA) $(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/ImportBeforeAfter/Microsoft.NuGet.ImportBefore.props $(DESTDIR)$(MSBUILD_EXTENSIONS_15_DIR)/Imports/Microsoft.Common.props/ImportBefore
	$(INSTALL_DATA) $(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/ImportBeforeAfter/Microsoft.NuGet.ImportAfter.targets $(DESTDIR)$(MSBUILD_EXTENSIONS_15_DIR)/Microsoft.Common.targets/ImportAfter

install-nuget-imports:
ifeq (14.0, $(XBUILD_VERSION))
	$(MKINSTALLDIRS) $(DESTDIR)$(XBUILD_BIN_DIR)/../Imports/Microsoft.Common.props/ImportBefore
	$(MKINSTALLDIRS) $(DESTDIR)$(XBUILD_BIN_DIR)/../Microsoft.Common.targets/ImportAfter
	$(INSTALL_DATA) $(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/ImportBeforeAfter/Microsoft.NuGet.ImportBefore.props $(DESTDIR)$(XBUILD_BIN_DIR)/../Imports/Microsoft.Common.props/ImportBefore
	$(INSTALL_DATA) $(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/ImportBeforeAfter/Microsoft.NuGet.ImportAfter.targets $(DESTDIR)$(XBUILD_BIN_DIR)/../Microsoft.Common.targets/ImportAfter
endif

# The .NETPortable,Version=v5.0 framework contains no assemblies, and essentially just fills the requirement
# for a framework moniker. When using it, assemblies are provided by NuGet packages such as .NETStandard.Library
install-pcl5-framework:
	$(MKINSTALLDIRS) $(DESTDIR)$(PCL5_FX_DIR)/RedistList
	$(INSTALL_DATA) $(PCL5_FX_SRC)/FrameworkList.xml $(DESTDIR)$(PCL5_FX_DIR)/RedistList/FrameworkList.xml

	$(MKINSTALLDIRS) $(DESTDIR)$(PCL5_FX_DIR)/SupportedFrameworks
	$(INSTALL_DATA) "$(PCL5_FX_SRC)/.NET Framework 4.6.xml" "$(DESTDIR)$(PCL5_FX_DIR)/SupportedFrameworks/.NET Framework 4.6.xml"
	$(INSTALL_DATA) "$(PCL5_FX_SRC)/ASP.NET Core 1.0.xml" "$(DESTDIR)$(PCL5_FX_DIR)/SupportedFrameworks/ASP.NET Core 1.0.xml"
	$(INSTALL_DATA) "$(PCL5_FX_SRC)/Windows Universal 10.0.xml" "$(DESTDIR)$(PCL5_FX_DIR)/SupportedFrameworks/Windows Universal 10.0.xml"

install-msbuild-specific-files:
	$(MKINSTALLDIRS) $(DESTDIR)$(mono_libdir)/mono/xbuild
	$(INSTALL_DATA) $(DENIED_ASSEMBLY_LIST_SRC) $(DESTDIR)$(mono_libdir)/mono/xbuild

EXTRA_DISTFILES = \
	data/xbuild.rsp \
	data/xbuild.exe.config.in \
	data/xbuild.exe.config_test.in \
	data/Microsoft.Build.xsd \
	data/2.0/Microsoft.Common.tasks \
	data/3.5/Microsoft.Common.tasks \
	data/4.0/Microsoft.Common.tasks \
	data/12.0/Microsoft.Common.tasks \
	data/14.0/Microsoft.Common.tasks \
	data/2.0/Microsoft.Common.targets \
	data/3.5/Microsoft.Common.targets \
	data/4.0/Microsoft.Common.targets \
	data/12.0/Microsoft.Common.targets \
	data/14.0/Microsoft.Common.targets \
	data/14.0/Microsoft.Common.props \
	data/2.0/Microsoft.CSharp.targets \
	data/3.5/Microsoft.CSharp.targets \
	data/4.0/Microsoft.CSharp.targets \
	data/12.0/Microsoft.CSharp.targets \
	data/14.0/Microsoft.CSharp.targets \
	data/Microsoft.VisualBasic.targets \
	data/MSBuild/Microsoft.Build.CommonTypes.xsd \
	data/MSBuild/Microsoft.Build.Core.xsd \
	data/Portable/Targets/Microsoft.Portable.Core.props \
	data/Portable/Targets/Microsoft.Portable.Core.targets \
	data/Portable/Targets/v4.0/Microsoft.Portable.Common.targets \
	data/Portable/Targets/v4.0/Microsoft.Portable.CSharp.targets \
	data/Portable/Targets/v4.0/Microsoft.Portable.VisualBasic.targets \
	data/Portable/Targets/v4.5/Microsoft.Portable.Common.targets \
	data/Portable/Targets/v4.5/Microsoft.Portable.CSharp.targets \
	data/Portable/Targets/v4.5/Microsoft.Portable.VisualBasic.targets \
	data/Portable/Targets/v4.6/Microsoft.Portable.Common.targets \
	data/Portable/Targets/v4.6/Microsoft.Portable.CSharp.targets \
	data/Portable/Targets/v4.6/Microsoft.Portable.VisualBasic.targets \
	data/Portable/Targets/v5.0/Microsoft.Portable.Common.targets \
	data/Portable/Targets/v5.0/Microsoft.Portable.CSharp.targets \
	data/Portable/Targets/v5.0/Microsoft.Portable.VisualBasic.targets \
	"data/Portable/Frameworks/v5.0/.NET Framework 4.6.xml" \
	"data/Portable/Frameworks/v5.0/ASP.NET Core 1.0.xml" \
	"data/Portable/Frameworks/v5.0/FrameworkList.xml" \
	"data/Portable/Frameworks/v5.0/Windows Universal 10.0.xml" \
	data/deniedAssembliesList.txt \
	frameworks/net_2.0.xml \
	frameworks/net_3.0.xml \
	frameworks/net_3.5.xml \
	frameworks/net_4.0.xml \
	frameworks/net_4.0_client.xml \
	frameworks/net_4.5.xml \
	frameworks/net_4.5.1.xml \
	frameworks/net_4.5.2.xml \
	frameworks/net_4.6.xml \
	frameworks/net_4.6.1.xml \
	frameworks/net_4.6.2.xml \
	frameworks/net_4.7.xml \
	frameworks/net_4.7.1.xml \
	frameworks/net_4.7.2.xml \
	targets/Microsoft.WebApplication.targets	\
	$(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/ImportBeforeAfter/Microsoft.NuGet.ImportBefore.props	\
	$(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/ImportBeforeAfter/Microsoft.NuGet.ImportAfter.targets	\
	$(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/Microsoft.NuGet.targets \
	$(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/Microsoft.NuGet.props \
	xbuild.make \
	xbuild_test.make
