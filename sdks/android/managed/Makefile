TOP=$(realpath $(CURDIR)/../../..)
include $(TOP)/sdks/paths.mk

BCL_DIR=$(TOP)/sdks/out/bcl/monodroid
MANAGED_DIR=$(TOP)/sdks/android/managed
ASSETS_DIR=$(TOP)/sdks/android/assets/asm

BCL_ASSEMBLIES= \
	mscorlib.dll \
	System.dll \
	System.Core.dll \
	System.Xml.dll \
	Mono.Security.dll

BCL_FILES=$(patsubst %,$(BCL_DIR)/%,$(BCL_ASSEMBLIES))

BCL_REFERENCES=$(patsubst %,-r:%,$(BCL_FILES))

NUNIT_ASSEMBLIES= \
	nunitlite.dll

NUNIT_FILES=$(patsubst %,$(MANAGED_DIR)/%,$(NUNIT_ASSEMBLIES))

NUNIT_REFERENCES=$(patsubst %,-r:%,$(NUNIT_FILES))

MINI_TEST_SOURCES= \
	TestDriver.cs \
	aot-tests.cs \
	arrays.cs \
	basic-calls.cs \
	basic-float.cs \
	basic-long.cs \
	basic-math.cs \
	basic.cs \
	exceptions.cs \
	generics.cs \
	gshared.cs \
	objects.cs

MINI_TEST_FILES= \
	$(patsubst %,$(TOP)/mono/mini/%,$(MINI_TEST_SOURCES)) \
	mini-test-runner.cs

BUILD_ARTIFACT=

all: .stamp-published

BUILD_ARTIFACT += $(MANAGED_DIR)/mini_tests.dll
$(MANAGED_DIR)/mini_tests.dll: $(MINI_TEST_FILES) $(BCL_FILES) $(NUNIT_FILES)
	mcs /nostdlib /unsafe -target:library -out:$@ -define:__MOBILE__ $(BCL_REFERENCES) $(NUNIT_REFERENCES) $(MINI_TEST_FILES)

#FIXME copy the resource assemblies
BUILD_ARTIFACT += $(MANAGED_DIR)/monodroid_corlib_test.dll
$(MANAGED_DIR)/monodroid_corlib_test.dll: $(BCL_FILES) $(NUNIT_FILES)
	$(MAKE) PROFILE='monodroid' -C $(TOP)/mcs/class/corlib build-test-lib
	cp $(TOP)/mcs/class/corlib/$(notdir $@) $(dir $@)

BUILD_ARTIFACT += $(MANAGED_DIR)/monodroid_System_test.dll
$(MANAGED_DIR)/monodroid_System_test.dll: $(BCL_FILES) $(NUNIT_FILES)
	$(MAKE) PROFILE='monodroid' -C $(TOP)/mcs/class/System build-test-lib
	cp $(TOP)/mcs/class/System/$(notdir $@) $(dir $@)

BUILD_ARTIFACT += $(NUNIT_FILES)
$(NUNIT_FILES): $(BCL_FILES)
	$(MAKE) PROFILE='monodroid' -C $(TOP)/mcs/tools/nunit-lite all
	cp $(TOP)/mcs/class/lib/monodroid/$(notdir $@) $(dir $@)

BUILD_ARTIFACT += $(MANAGED_DIR)/main.exe
$(MANAGED_DIR)/main.exe: main.cs mini_tests.dll $(BCL_FILES) $(NUNIT_FILES)
	mcs /nostdlib -out:$@ $(BCL_REFERENCES) $(NUNIT_REFERENCES) main.cs

BUILD_ARTIFACT += $(MANAGED_DIR)/Mono.Android.dll
$(MANAGED_DIR)/Mono.Android.dll: fake-monodroid.cs
	mcs /nostdlib -target:library -out:$@ $(BCL_REFERENCES) fake-monodroid.cs

clean:
	rm $(BUILD_ARTIFACT)

$(ASSETS_DIR):
	mkdir -p $@

.stamp-published: $(BUILD_ARTIFACT) $(ASSETS_DIR) Makefile
	cp $(BUILD_ARTIFACT) $(BCL_FILES) $(NUNIT_FILES) $(ASSETS_DIR)
	touch $@
