thisdir = class/Microsoft.Build.Utilities
SUBDIRS = 
include ../../build/rules.make

XBUILD_DIR=$(topdir)/tools/xbuild
include $(XBUILD_DIR)/xbuild.make

LIBRARY = Microsoft.Build.Utilities.dll
LIBRARY_NAME = Microsoft.Build.Utilities$(NAME_SUFFIX).dll

LIB_REFS = $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core $(PARENT_PROFILE)System.Xml $(XBUILD_FRAMEWORK)
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS =

TEST_RESX_RESOURCES = Test/Microsoft.Build.Utilities/Strings.resources

TEST_LIB_REFS = $(XBUILD_ENGINE) $(XBUILD_FRAMEWORK) $(PARENT_PROFILE)System $(PARENT_PROFILE)System.Core
TEST_MCS_FLAGS = $(TEST_RESX_RESOURCES:%=-resource:%)
TEST_NUNITLITE_APP_CONFIG_RUNTIME=Test/test-config-file-$(PROFILE)

include $(XBUILD_DIR)/xbuild_test.make
include ../../build/library.make

EXTRA_DISTFILES = $(TEST_RESX_RESOURCES:.resources=.resx)

CLEAN_FILES += $(TEST_RESX_RESOURCES)

.NOTPARALLEL: $(TEST_RESX_RESOURCES)

$(TEST_RESX_RESOURCES): %.resources: %.resx
	$(RESGEN) $< || cp $@.prebuilt $@

$(test_lib_output): $(TEST_RESX_RESOURCES) $(test_lib_dir)
