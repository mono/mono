thisdir = tools/msbuild
SUBDIRS = 
include ../../build/rules.make
NO_TESTS = yes

BUILD_FRAMEWORK = Microsoft.Build.Framework
BUILD_NEW_ENGINE = Microsoft.Build
INSTALL_FRAMEWORK_VERSION = $(FRAMEWORK_VERSION)

include ../xbuild/xbuild.make

ifeq (3.5, $(FRAMEWORK_VERSION))
NAME_SUFFIX = .v3.5
ASSEMBLY_VERSION = 3.5.0.0
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
NAME_SUFFIX = .v4.0
ASSEMBLY_VERSION = 4.0.0.0
INSTALL_FRAMEWORK_VERSION = 4.0
endif
endif

LOCAL_MCS_FLAGS =
LIB_REFS = $(BUILD_FRAMEWORK) $(BUILD_NEW_ENGINE)
PROGRAM = msbuild.exe

include ../../build/executable.make

XBUILD_DIR=.
XBUILD_FRAMEWORKS_DIR=$(mono_libdir)/mono/xbuild-frameworks/.NETFramework/
REDISTLIST_DIR=$(XBUILD_FRAMEWORKS_DIR)/v$(FRAMEWORK_VERSION)/RedistList
#include $(XBUILD_DIR)/xbuild_targets.make

install-local: install-extras

install-extras: install-bin-data

install-bin-data:
	$(INSTALL_DATA) data/msbuild.rsp $(DESTDIR)$(XBUILD_BIN_DIR)

EXTRA_DISTFILES = \
	data/msbuild.rsp

