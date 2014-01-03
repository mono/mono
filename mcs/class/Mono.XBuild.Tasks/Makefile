thisdir = class/Mono.XBuild.Tasks
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.XBuild.Tasks.dll

LIB_MCS_FLAGS = \
	/r:$(corlib)				\
	/r:System.dll				\
	/r:System.Xml.dll

export TESTING_MONO=a
XBUILD_DIR=../../tools/xbuild
include $(XBUILD_DIR)/xbuild_targets.make

include ../../build/library.make
