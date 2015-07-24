thisdir = class/Microsoft.Build.Utilities
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Utilities.dll
LIBRARY_NAME = Microsoft.Build.Utilities$(NAME_SUFFIX).dll

LIB_REFS = System System.Core System.Xml
LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Core.dll			\
	/r:System.Xml.dll			\
	/r:$(XBUILD_FRAMEWORK)

TEST_MCS_FLAGS = /r:$(XBUILD_ENGINE) /r:$(XBUILD_FRAMEWORK) -r:System.dll -r:System.Core.dll

include $(XBUILD_DIR)/xbuild_test.make
include ../../build/library.make
