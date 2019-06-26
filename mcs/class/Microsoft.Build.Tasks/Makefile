thisdir = class/Microsoft.Build.Tasks
SUBDIRS =
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Tasks.dll

LIBRARY_NAME = Microsoft.Build.Tasks$(NAME_SUFFIX).dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core $(PARENT_PROFILE)System.Xml $(PARENT_PROFILE)System.Windows.Forms \
			$(XBUILD_UTILITIES) $(XBUILD_FRAMEWORK) $(XBUILD_ENGINE) $(XBUILD_TASKS)
KEYFILE = ../msfinal.pub

TEST_RESOURCE_FILES = \
	Test/resources/test.cs \
	Test/resources/test.dll \
	Test/resources/Sample.cs \
	Test/resources/Sample.vb \
	Test/resources/junk.txt \
	Test/resources/binary/FancyStuff.dll \
	Test/resources/binary/FancyStuff.dll.mdb \
	Test/resources/binary/SimpleWrite.dll \
	Test/resources/binary/SimpleWrite.dll.mdb \
	Test/resources/binary/Testing.dll \
	Test/resources/binary/Testing.dll.mdb \
	Test/resources/binary/XbuildReferenceBugTest.exe \
	Test/resources/binary/XbuildReferenceBugTest.exe.mdb \
	Test/resources/binary/XbuildReferenceBugTest2.exe

TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))
TEST_LIB_REFS = $(PARENT_PROFILE)System.Xml $(XBUILD_ENGINE) $(XBUILD_FRAMEWORK) $(XBUILD_TASKS) $(XBUILD_UTILITIES) $(PARENT_PROFILE)System.Core

TEST_NUNITLITE_APP_CONFIG_RUNTIME=Test/test-config-file-$(PROFILE)

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
TEST_LIB_REFS += Microsoft.Build
endif

EXTRA_DISTFILES = $(filter-out Test/resources/test.dll, $(TEST_RESOURCE_FILES))

Test/resources/test.dll: Test/resources/test.cs
	$(CSCOMPILE) -r:$(topdir)/class/lib/$(PROFILE)/$(PARENT_PROFILE)mscorlib.dll -target:library /out:$@ $<

clean-test-resources:
	rm -f Test/resources/test.dll

test-local: Test/resources/test.dll

clean-local: clean-test-resources

include $(XBUILD_DIR)/xbuild_test.make
include ../../build/library.make
