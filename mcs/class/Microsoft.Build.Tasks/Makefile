thisdir = class/Microsoft.Build.Tasks
SUBDIRS =
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Tasks.dll

LIBRARY_NAME = Microsoft.Build.Tasks$(NAME_SUFFIX).dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core $(PARENT_PROFILE)System.Xml $(PARENT_PROFILE)System.Windows.Forms \
			$(XBUILD_UTILITIES) $(XBUILD_FRAMEWORK) $(XBUILD_ENGINE) $(XBUILD_TASKS)

TEST_MCS_FLAGS =
TEST_LIB_REFS = $(PARENT_PROFILE)System.Xml $(XBUILD_ENGINE) $(XBUILD_FRAMEWORK) $(XBUILD_TASKS) $(XBUILD_UTILITIES) $(PARENT_PROFILE)System.Core

ifeq (4, $(FRAMEWORK_VERSION_MAJOR))
TEST_LIB_REFS += Microsoft.Build
endif

EXTRA_DISTFILES = \
	Test/resources/test.cs \
	Test/resources/Sample.cs \
	Test/resources/Sample.vb \
	Test/resources/junk.txt \
	Test/test-config-file*

Test/resources/test.dll: Test/resources/test.cs
	$(CSCOMPILE) -target:library /out:$@ $<

clean-test-resources:
	rm -f Test/resources/test.dll

test-local: Test/resources/test.dll

clean-local: clean-test-resources

include $(XBUILD_DIR)/xbuild_test.make
include ../../build/library.make
