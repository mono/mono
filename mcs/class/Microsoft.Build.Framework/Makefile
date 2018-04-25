thisdir = class/Microsoft.Build.Framework
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Framework.dll

LIB_REFS = $(PARENT_PROFILE)System
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS =

TEST_NUNITLITE_APP_CONFIG_RUNTIME=Test/test-config-file-$(PROFILE)

EXTRA_DISTFILES = \
	Mono.XBuild.Framework/AssemblyLoadInfo.cs

include ../../build/library.make
include $(XBUILD_DIR)/xbuild_test.make
