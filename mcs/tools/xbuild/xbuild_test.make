XBUILD_DATA_DIR=$(topdir)/tools/xbuild/data
XBUILD_PROFILE_DIR=$(topdir)/class/lib/$(PROFILE)

# makes xbuild look in the class/lib/$PROFILE build directories for targets etc
export TESTING_MONO=a

ifeq (4.0, $(FRAMEWORK_VERSION))
test-local: xbuild-net4-fail
else
test-local: copy-targets $(test_lib).config
endif

xbuild-net4-fail:
	@echo "The net_4_0 profile contains reference assemblies only and cannot be installed/tested as an xbuild toolset"
	@exit 1

copy-targets:
	cp $(XBUILD_DATA_DIR)/$(XBUILD_VERSION)/Microsoft.Common.targets $(XBUILD_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/$(XBUILD_VERSION)/Microsoft.Common.tasks $(XBUILD_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/Microsoft.CSharp.targets $(XBUILD_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/Microsoft.VisualBasic.targets $(XBUILD_PROFILE_DIR)

clean-local: clean-target-files clean-test-config

clean-target-files:
	rm -f $(XBUILD_PROFILE_DIR)/Microsoft.Common.targets
	rm -f $(XBUILD_PROFILE_DIR)/Microsoft.Common.tasks
	rm -f $(XBUILD_PROFILE_DIR)/Microsoft.CSharp.targets
	rm -f $(XBUILD_PROFILE_DIR)/Microsoft.VisualBasic.targets

$(test_lib).config: $(XBUILD_DATA_DIR)/xbuild.exe.config.in
	sed -e 's/@ASM_VERSION@/$(XBUILD_ASSEMBLY_VERSION)/g' $(XBUILD_DATA_DIR)/xbuild.exe.config.in > $(test_lib).config

clean-test-config:
	rm -f $(test_lib).config

#allow tests to find older versions of libs
ifneq (2.0, $(XBUILD_VERSION))
TEST_MONO_PATH := $(topdir)/class/lib/net_2_0
ifneq (3.5, $(XBUILD_VERSION))
TEST_MONO_PATH := $(topdir)/class/lib/net_3_5$(PLATFORM_PATH_SEPARATOR)$(TEST_MONO_PATH)
ifneq (4.0, $(XBUILD_VERSION))
TEST_MONO_PATH := $(topdir)/class/lib/net_4_5$(PLATFORM_PATH_SEPARATOR)$(TEST_MONO_PATH)
endif
endif
endif
