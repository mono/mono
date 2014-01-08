ifneq (2.0, $(XBUILD_VERSION))
NAME_SUFFIX = .v$(XBUILD_VERSION)
endif

XBUILD_FRAMEWORK := $(topdir)/class/lib/$(PROFILE)/Microsoft.Build.Framework.dll
XBUILD_ENGINE := $(topdir)/class/lib/$(PROFILE)/Microsoft.Build.Engine.dll
XBUILD_UTILITIES := $(topdir)/class/lib/$(PROFILE)/Microsoft.Build.Utilities$(NAME_SUFFIX).dll
XBUILD_TASKS := $(topdir)/class/lib/$(PROFILE)/Mono.XBuild.Tasks.dll

XBUILD_ASSEMBLY_VERSION = $(XBUILD_VERSION).0.0

XBUILD_BIN_DIR = $(mono_libdir)/mono/$(FRAMEWORK_VERSION)

ifneq (2.0, $(XBUILD_VERSION))
ifneq (3.5, $(XBUILD_VERSION))
ifneq (4.0, $(XBUILD_VERSION))

XBUILD_BIN_DIR = $(mono_libdir)/mono/xbuild/$(XBUILD_VERSION)/bin

PROGRAM_INSTALL_DIR = $(XBUILD_BIN_DIR)
LIBRARY_PACKAGE = xbuild/$(XBUILD_VERSION)/bin

endif
endif
endif
