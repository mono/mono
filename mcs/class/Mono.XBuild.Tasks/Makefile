thisdir = class/Mono.XBuild.Tasks
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Mono.XBuild.Tasks.dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Xml
KEYFILE = ../mono.pub
LIB_MCS_FLAGS =

TEST_NUNITLITE_APP_CONFIG_RUNTIME=Test/test-config-file-$(PROFILE)

include $(XBUILD_DIR)/xbuild_test.make

include ../../build/library.make
