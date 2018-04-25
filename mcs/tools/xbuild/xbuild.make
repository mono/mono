ifneq (2.0, $(XBUILD_VERSION))
NAME_SUFFIX = .v$(XBUILD_VERSION)
endif

ifeq (14.0, $(XBUILD_VERSION))
NAME_SUFFIX = .Core
endif

XBUILD_FRAMEWORK := Microsoft.Build.Framework
XBUILD_ENGINE := Microsoft.Build.Engine
XBUILD_UTILITIES := Microsoft.Build.Utilities$(NAME_SUFFIX)
XBUILD_TASKS := Mono.XBuild.Tasks
XBUILD_MSTASKS := Microsoft.Build.Tasks$(NAME_SUFFIX)
ifeq (14.0, $(XBUILD_VERSION))
XBUILD_MSTASKS := Microsoft.Build.Tasks.Core
endif

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
