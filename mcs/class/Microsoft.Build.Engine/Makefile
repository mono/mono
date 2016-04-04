thisdir = class/Microsoft.Build.Engine
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Engine.dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core $(PARENT_PROFILE)System.Xml $(XBUILD_FRAMEWORK) $(XBUILD_UTILITIES)
LIB_MCS_FLAGS =

TEST_MCS_FLAGS =
TEST_LIB_REFS = $(XBUILD_FRAMEWORK) $(XBUILD_UTILITIES) $(PARENT_PROFILE)System.Xml

EXTRA_DISTFILES = \
	Test/resources/TestTasks.cs		\
	Test/resources/*.*proj	\
	Test/resources/*.csproj

CLEAN_FILES = Test/resources/TestTasks-$(PROFILE).dll Test/resources/TestTasks-$(PROFILE).dll.mdb Test/resources/TestTasks-$(PROFILE).pdb

Test/resources/TestTasks-$(PROFILE).dll: Test/resources/TestTasks.cs
	$(CSCOMPILE) /out:$@ Test/resources/TestTasks.cs /r:$(topdir)/class/lib/$(PROFILE)/$(XBUILD_FRAMEWORK).dll /r:$(topdir)/class/lib/$(PROFILE)/$(XBUILD_UTILITIES).dll /target:library

test-local: compile-resources

compile-resources: Test/resources/TestTasks-$(PROFILE).dll
	cp Test/resources/TestTasks-$(PROFILE).dll Test/resources/TestTasks.dll
	cp Test/resources/TestTasks-$(PROFILE).dll.mdb Test/resources/TestTasks.dll.mdb
#	cp Test/resources/TestTasks-$(PROFILE).pdb Test/resources/TestTasks.pdb

include $(XBUILD_DIR)/xbuild_test.make
include ../../build/library.make
