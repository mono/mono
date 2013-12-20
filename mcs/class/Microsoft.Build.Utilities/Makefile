thisdir = class/Microsoft.Build.Utilities
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Utilities.dll
LIBRARY_NAME = Microsoft.Build.Utilities$(NAME_SUFFIX).dll

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:$(XBUILD_FRAMEWORK)

TEST_MCS_FLAGS = /r:$(XBUILD_FRAMEWORK) -r:System.dll -r:System.Core.dll

export TESTING_MONO=a
include $(XBUILD_DIR)/xbuild_targets.make

include ../../build/library.make
