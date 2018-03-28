thisdir = class/Microsoft.NuGet.Build.Tasks
SUBDIRS =
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

NUGET_BUILDTASKS_REPO_DIR=$(topdir)/../external/nuget-buildtasks
NUGET_BUILDTASKS_TARGETS_DIR = $(mono_libdir)/mono/xbuild/Microsoft/NuGet

LIBRARY = Microsoft.NuGet.Build.Tasks.dll
LIBRARY_INSTALL_DIR = $(NUGET_BUILDTASKS_TARGETS_DIR)

KEY_FILE = $(NUGET_BUILDTASKS_REPO_DIR)/build/PublicKey.snk
SIGN_FLAGS = /delaysign /keyfile:$(KEY_FILE)

RESOURCE_DEFS = Microsoft.NuGet.Build.Tasks.Strings,$(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/Strings.resx

LIB_REFS = $(PARENT_PROFILE)System \
	$(PARENT_PROFILE)System.Core \
	$(PARENT_PROFILE)System.Data \
	$(PARENT_PROFILE)System.Xml \
	$(PARENT_PROFILE)System.Xml.Linq \
	$(PARENT_PROFILE)System.Runtime.Serialization \
	$(XBUILD_UTILITIES) \
	$(XBUILD_FRAMEWORK)

LIB_MCS_FLAGS = \
	-nowarn:3021		\
	$(SIGN_FLAGS)

EXTRA_DISTFILES = \
	$(NUGET_BUILDTASKS_REPO_DIR)/build/PublicKey.snk

include ../../build/library.make
