thisdir = class/Mono.XBuild.Utilities
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Mono.XBuild.Utilities.dll
LIBRARY_NAME = Mono.XBuild.Utilities$(NAME_SUFFIX).dll

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Xml.dll			\
	/r:$(XBUILD_FRAMEWORK)

include $(XBUILD_DIR)/xbuild_test.make

include ../../build/library.make
