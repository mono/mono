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

LIB_REFS = System System.Core System.Data System.Xml System.Xml.Linq System.Runtime.Serialization
LIB_MCS_FLAGS = \
	-nowarn:3021		\
	$(SIGN_FLAGS)		\
	-resource:Microsoft.NuGet.Build.Tasks.Strings.resources		\
	/r:$(XBUILD_UTILITIES)		\
	/r:$(XBUILD_FRAMEWORK)

CLEAN_FILES = Microsoft.NuGet.Build.Tasks.Strings.resources

EXTRA_DISTFILES = \
	$(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/Strings.resx \
	$(NUGET_BUILDTASKS_REPO_DIR)/build/PublicKey.snk

include ../../build/library.make

$(build_lib): Microsoft.NuGet.Build.Tasks.Strings.resources

Microsoft.NuGet.Build.Tasks.Strings.resources: $(NUGET_BUILDTASKS_REPO_DIR)/src/Microsoft.NuGet.Build.Tasks/Strings.resx
	MONO_PATH="$(topdir)/class/lib/$(BOOTSTRAP_PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(topdir)/class/lib/net_4_x/resgen.exe "$<" "$@"
