thisdir = class/Microsoft.Build.Engine
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Engine.dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core $(PARENT_PROFILE)System.Xml $(XBUILD_FRAMEWORK) $(XBUILD_UTILITIES)
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS =

TEST_RESOURCE_FILES = \
	$(wildcard Test/resources/*.csproj)

TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))
TEST_LIB_REFS = $(XBUILD_FRAMEWORK) $(XBUILD_UTILITIES) $(PARENT_PROFILE)System.Xml
TEST_NUNITLITE_APP_CONFIG_RUNTIME=Test/test-config-file-$(PROFILE)

EXTRA_DISTFILES = \
	Test/resources/TestTasks.cs		\
	Test/resources/*.csproj

CLEAN_FILES = Test/resources/TestTasks-$(PROFILE).dll Test/resources/TestTasks-$(PROFILE).dll.mdb Test/resources/TestTasks-$(PROFILE).pdb

Test/resources/TestTasks-$(PROFILE).dll: Test/resources/TestTasks.cs
	$(CSCOMPILE) /out:$@ Test/resources/TestTasks.cs /r:$(topdir)/class/lib/$(PROFILE)/$(PARENT_PROFILE)/mscorlib.dll /r:$(topdir)/class/lib/$(PROFILE)/$(XBUILD_FRAMEWORK).dll /r:$(topdir)/class/lib/$(PROFILE)/$(XBUILD_UTILITIES).dll /target:library

TEST_MCS_FLAGS += -resource:Test/resources/TestTasks-$(PROFILE).dll,Test/resources/TestTasks.dll

ifndef MCS_MODE
TEST_MCS_FLAGS += -resource:Test/resources/TestTasks-$(PROFILE).pdb,Test/resources/TestTasks.pdb
endif

test-local: Test/resources/TestTasks-$(PROFILE).dll

include $(XBUILD_DIR)/xbuild_test.make
include ../../build/library.make
