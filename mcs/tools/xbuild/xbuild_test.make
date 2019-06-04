XBUILD_DATA_DIR=$(topdir)/tools/xbuild/data
XBUILD_TESTING_DIR=$(topdir)/class/lib/net_4_x/tests/xbuild

# makes xbuild look in the class/lib/$PROFILE build directories for targets etc
export TESTING_MONO=a
export MSBuildExtensionsPath=$(XBUILD_TESTING_DIR)/extensions
export XBUILD_FRAMEWORK_FOLDERS_PATH=$(XBUILD_TESTING_DIR)/frameworks

test-local: copy-targets copy-data copy-frameworks Test/test-config-file-$(PROFILE)
clean-local: clean-targets clean-data clean-frameworks clean-test-config

Test/test-config-file-$(PROFILE): $(XBUILD_DATA_DIR)/xbuild.exe.config_test.in
	sed -e 's/@ASM_VERSION@/$(XBUILD_ASSEMBLY_VERSION)/g' $(XBUILD_DATA_DIR)/xbuild.exe.config_test.in > Test/test-config-file-$(PROFILE)

clean-test-config:
	rm -f Test/test-config-file-$(PROFILE)

copy-targets: copy-targets-$(XBUILD_VERSION)

clean-targets: clean-targets-$(XBUILD_VERSION)

XBUILD_4_0_PROFILE_DIR=$(topdir)/class/lib/net_4_x
XBUILD_12_0_PROFILE_DIR=$(topdir)/class/lib/xbuild_12
XBUILD_14_0_PROFILE_DIR=$(topdir)/class/lib/xbuild_14

copy-data:
	mkdir -p $(XBUILD_TESTING_DIR)/extensions
	cp -R -L $(XBUILD_DATA_DIR)/. $(XBUILD_TESTING_DIR)/extensions

copy-frameworks:
	mkdir -p $(XBUILD_TESTING_DIR)/frameworks
	cp -R -L $(topdir)/class/Microsoft.Build/xbuild-testing/. $(XBUILD_TESTING_DIR)/frameworks

copy-targets-4.0:
	cp $(XBUILD_DATA_DIR)/4.0/Microsoft.Common.targets $(XBUILD_4_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/4.0/Microsoft.Common.tasks $(XBUILD_4_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/4.0/Microsoft.CSharp.targets $(XBUILD_4_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/Microsoft.VisualBasic.targets $(XBUILD_4_0_PROFILE_DIR)

copy-targets-12.0:
	cp $(XBUILD_DATA_DIR)/12.0/Microsoft.Common.targets $(XBUILD_12_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/12.0/Microsoft.Common.tasks $(XBUILD_12_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/12.0/Microsoft.CSharp.targets $(XBUILD_12_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/Microsoft.VisualBasic.targets $(XBUILD_12_0_PROFILE_DIR)

copy-targets-14.0:
	cp $(XBUILD_DATA_DIR)/14.0/Microsoft.Common.targets $(XBUILD_14_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/14.0/Microsoft.Common.tasks $(XBUILD_14_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/14.0/Microsoft.CSharp.targets $(XBUILD_14_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/Microsoft.VisualBasic.targets $(XBUILD_14_0_PROFILE_DIR)

clean-data:
	rm -rf $(XBUILD_TESTING_DIR)/extensions

clean-frameworks:
	rm -rf $(XBUILD_TESTING_DIR)/frameworks

clean-targets-4.0:
	rm -f $(XBUILD_4_0_PROFILE_DIR)/Microsoft.Common.targets
	rm -f $(XBUILD_4_0_PROFILE_DIR)/Microsoft.Common.tasks
	rm -f $(XBUILD_4_0_PROFILE_DIR)/Microsoft.CSharp.targets
	rm -f $(XBUILD_4_0_PROFILE_DIR)/Microsoft.VisualBasic.targets

clean-targets-12.0:
	rm -f $(XBUILD_12_0_PROFILE_DIR)/Microsoft.Common.targets
	rm -f $(XBUILD_12_0_PROFILE_DIR)/Microsoft.Common.tasks
	rm -f $(XBUILD_12_0_PROFILE_DIR)/Microsoft.CSharp.targets
	rm -f $(XBUILD_12_0_PROFILE_DIR)/Microsoft.VisualBasic.targets

clean-targets-14.0:
	rm -f $(XBUILD_14_0_PROFILE_DIR)/Microsoft.Common.targets
	rm -f $(XBUILD_14_0_PROFILE_DIR)/Microsoft.Common.tasks
	rm -f $(XBUILD_14_0_PROFILE_DIR)/Microsoft.CSharp.targets
	rm -f $(XBUILD_14_0_PROFILE_DIR)/Microsoft.VisualBasic.targets

#allow tests to find older versions of libs and targets
ifneq (4.0, $(XBUILD_VERSION))
TEST_MONO_PATH := $(topdir)/class/lib/net_4_x$(PLATFORM_PATH_SEPARATOR)$(TEST_MONO_PATH)
copy-targets: copy-targets-4.0
clean-targets: clean-targets-4.0
endif
