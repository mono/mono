thisdir = class/Mono.XBuild.Tasks
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Mono.XBuild.Tasks.dll

LIB_REFS = System System.Xml
LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Xml.dll

include $(XBUILD_DIR)/xbuild_test.make

include ../../build/library.make
