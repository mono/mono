XBUILD_DATA_DIR=$(topdir)/tools/xbuild/data
XBUILD_PROFILE_DIR=$(topdir)/class/lib/$(PROFILE)

# makes xbuild look in the class/lib/$PROFILE build directories for targets etc
export TESTING_MONO=a

ifeq (4.0, $(FRAMEWORK_VERSION))
NO_TEST=true
else
test-local: copy-targets $(test_lib).config
clean-local: clean-targets clean-test-config
endif

xbuild-net4-fail:
	@echo "The net_4_0 profile contains reference assemblies only and cannot be installed/tested as an xbuild toolset"
	@exit 1

$(test_lib).config: $(XBUILD_DATA_DIR)/xbuild.exe.config.in
	sed -e 's/@ASM_VERSION@/$(XBUILD_ASSEMBLY_VERSION)/g' $(XBUILD_DATA_DIR)/xbuild.exe.config.in > $(test_lib).config

clean-test-config:
	rm -f $(test_lib).config

copy-targets: copy-targets-$(XBUILD_VERSION)

clean-targets: clean-targets-$(XBUILD_VERSION)

XBUILD_2_0_PROFILE_DIR=$(topdir)/class/lib/net_2_0
XBUILD_3_5_PROFILE_DIR=$(topdir)/class/lib/net_3_5
XBUILD_4_0_PROFILE_DIR=$(topdir)/class/lib/net_4_5
XBUILD_12_0_PROFILE_DIR=$(topdir)/class/lib/xbuild_12

copy-targets-2.0:
	cp $(XBUILD_DATA_DIR)/2.0/Microsoft.Common.targets $(XBUILD_2_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/2.0/Microsoft.Common.tasks $(XBUILD_2_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/2.0/Microsoft.CSharp.targets $(XBUILD_2_0_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/Microsoft.VisualBasic.targets $(XBUILD_2_0_PROFILE_DIR)

copy-targets-3.5:
	cp $(XBUILD_DATA_DIR)/3.5/Microsoft.Common.targets $(XBUILD_3_5_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/3.5/Microsoft.Common.tasks $(XBUILD_3_5_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/3.5/Microsoft.CSharp.targets $(XBUILD_3_5_PROFILE_DIR)
	cp $(XBUILD_DATA_DIR)/Microsoft.VisualBasic.targets $(XBUILD_3_5_PROFILE_DIR)

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

clean-targets-2.0:
	rm -f $(XBUILD_2_0_PROFILE_DIR)/Microsoft.Common.targets
	rm -f $(XBUILD_2_0_PROFILE_DIR)/Microsoft.Common.tasks
	rm -f $(XBUILD_2_0_PROFILE_DIR)/Microsoft.CSharp.targets
	rm -f $(XBUILD_2_0_PROFILE_DIR)/Microsoft.VisualBasic.targets

clean-targets-3.5:
	rm -f $(XBUILD_3_5_PROFILE_DIR)/Microsoft.Common.targets
	rm -f $(XBUILD_3_5_PROFILE_DIR)/Microsoft.Common.tasks
	rm -f $(XBUILD_3_5_PROFILE_DIR)/Microsoft.CSharp.targets
	rm -f $(XBUILD_3_5_PROFILE_DIR)/Microsoft.VisualBasic.targets

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

#allow tests to find older versions of libs and targets
ifneq (2.0, $(XBUILD_VERSION))
TEST_MONO_PATH := $(topdir)/class/lib/net_2_0
copy-targets: copy-targets-2.0
clean-targets: clean-targets-2.0
ifneq (3.5, $(XBUILD_VERSION))
TEST_MONO_PATH := $(topdir)/class/lib/net_3_5$(PLATFORM_PATH_SEPARATOR)$(TEST_MONO_PATH)
copy-targets: copy-targets-3.5
clean-targets: clean-targets-3.5
ifneq (4.0, $(XBUILD_VERSION))
TEST_MONO_PATH := $(topdir)/class/lib/net_4_5$(PLATFORM_PATH_SEPARATOR)$(TEST_MONO_PATH)
copy-targets: copy-targets-4.0
clean-targets: clean-targets-4.0
endif
endif
endif
