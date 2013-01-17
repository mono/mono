thisdir = class/Microsoft.Build.Utilities
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Build.Utilities.dll
BUILD_FRAMEWORK = Microsoft.Build.Framework.dll

ifeq (3.5, $(FRAMEWORK_VERSION))
LIBRARY_NAME = Microsoft.Build.Utilities.v3.5.dll
BUILD_FRAMEWORK := $(topdir)/class/lib/$(PROFILE)/$(BUILD_FRAMEWORK)
else
ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
LIBRARY_NAME = Microsoft.Build.Utilities.v4.0.dll
endif
endif

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:$(BUILD_FRAMEWORK)

TEST_MCS_FLAGS = /r:$(BUILD_FRAMEWORK) -r:System.dll

export TESTING_MONO=a
XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make

include ../../build/library.make
