TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/paths.mk
ADB       = $(SDK_DIR)/platform-tools/adb
ANDROID   = $(SDK_DIR)/tools/android
ANT       = ant
NDK_BUILD = $(NDK_DIR)/ndk-build

PACKAGE   = org.mono.android.AndroidTestRunner
RUNNER    = org.mono.android.AndroidRunner
ACTIVITY  = org.mono.android.AndroidActivity

APK_DIR=apk

BCL_CHECKS= \
	corlib \
	System \
	System.Core \
	System.Data \
	System.IO.Compression.FileSystem \
	System.IO.Compression \
	System.Json \
	System.Net.Http \
	System.Numerics \
	System.Runtime.Serialization \
	System.ServiceModel.Web \
	System.Transactions \
	System.Xml \
	System.Xml.Linq \
	Mono.CSharp \
	Mono.Data.Sqlite \
	Mono.Data.Tds \
	Mono.Security

CHECKS= \
	mini \
	$(BCL_CHECKS)

.PHONY: all
all: package

#This is hilarious, but we can't clean unless we got the 2 property files around
.PHONY: clean
clean:
	rm -rf $(APK_DIR)

.PHONY: check
check: $(patsubst %,check-%,$(CHECKS))

## Helper targets

.PHONY: undeploy
undeploy:
	$(ADB) uninstall $(PACKAGE)

.PHONY: kill
kill:
	$(ADB) shell am force-stop $(PACKAGE)

.PHONY: logcat
logcat:
	$(ADB) logcat

.PHONY: shell
shell:
	$(ADB) shell

## Check targets

BCL_LIBS= \
	mscorlib.dll \
	System.dll \
	System.Core.dll \
	System.Xml.dll \
	System.Data.dll \
	System.IO.Compression.FileSystem.dll \
	System.IO.Compression.dll \
	System.Json.dll \
	System.Net.Http.dll \
	System.Numerics.dll \
	System.Runtime.Serialization.dll \
	System.ServiceModel.Web.dll \
	System.Transactions.dll \
	System.Xml.Linq.dll \
	Mono.CSharp.dll \
	Mono.Data.Sqlite.dll \
	Mono.Data.Tds.dll \
	Mono.Security.dll

BCL_REFERENCES=$(patsubst %,-r:$(TOP)/sdks/out/bcl/monodroid/%,$(BCL_LIBS))

NUNIT_LIBS= \
	nunitlite.dll

NUNIT_REFERENCES=$(patsubst %,-r:$(TOP)/sdks/out/bcl/monodroid/%,$(NUNIT_LIBS))

$(APK_DIR) $(APK_DIR)/jni $(APK_DIR)/jni/armeabi-v7a $(APK_DIR)/jni/arm64-v8a $(APK_DIR)/jni/x86 $(APK_DIR)/jni/x86_64 $(APK_DIR)/assets $(APK_DIR)/assets/asm:
	mkdir -p $@

$(APK_DIR)/AndroidManifest.xml: $(APK_DIR)/%: % | $(APK_DIR)
	cp $< $@

$(APK_DIR)/build.xml $(APK_DIR)/local.properties $(APK_DIR)/project.properties: $(APK_DIR)/AndroidManifest.xml | $(APK_DIR)
	cd $(APK_DIR) && $(ANDROID) update project -p . -t "android-14"

.PHONY: res/ src/
$(APK_DIR)/res/ $(APK_DIR)/src/: $(APK_DIR)/%: % | $(APK_DIR)
	cp -r $< $@

$(APK_DIR)/jni/Android.mk $(APK_DIR)/jni/Application.mk $(APK_DIR)/jni/runtime-bootstrap.c: $(APK_DIR)/%: % | $(APK_DIR)/jni
	cp $< $@

MONO_LIBS= \
	libmonosgen-2.0.so \
	libMonoPosixHelper.so

$(patsubst %,$(APK_DIR)/jni/armeabi-v7a/%,$(MONO_LIBS)): $(APK_DIR)/jni/armeabi-v7a/%: $(TOP)/sdks/out/android-arm/lib/% | $(APK_DIR)/jni/armeabi-v7a
	cp $< $@

$(patsubst %,$(APK_DIR)/jni/arm64-v8a/%,$(MONO_LIBS)): $(APK_DIR)/jni/arm64-v8a/%: $(TOP)/sdks/out/android-arm64/lib/% | $(APK_DIR)/jni/arm64-v8a
	cp $< $@

$(patsubst %,$(APK_DIR)/jni/x86/%,$(MONO_LIBS)): $(APK_DIR)/jni/x86/%: $(TOP)/sdks/out/android-x86/lib/% | $(APK_DIR)/jni/x86
	cp $< $@

$(patsubst %,$(APK_DIR)/jni/x86_64/%,$(MONO_LIBS)): $(APK_DIR)/jni/x86_64/%: $(TOP)/sdks/out/android-x86_64/lib/% | $(APK_DIR)/jni/x86_64
	cp $< $@

$(APK_DIR)/assets/mconfig: machine.config | $(APK_DIR)/assets
	cp $< $@

$(APK_DIR)/assets/asm/main.exe: managed/main.cs | $(APK_DIR)/assets/asm
	mcs /nostdlib -unsafe -out:$@ $(BCL_REFERENCES) $(NUNIT_REFERENCES) $<

$(APK_DIR)/assets/asm/Mono.Android.dll: managed/fake-monodroid.cs | $(APK_DIR)/assets/asm
	mcs /nostdlib -target:library -out:$@ $(BCL_REFERENCES) $<

$(patsubst %,$(APK_DIR)/assets/asm/%,$(BCL_LIBS)) $(patsubst %,$(APK_DIR)/assets/asm/%,$(NUNIT_LIBS)): $(APK_DIR)/assets/asm/%: $(TOP)/sdks/out/bcl/monodroid/% | $(APK_DIR)/assets/asm
	cp $< $@

.PHONY: package
package: $(APK_DIR)/AndroidManifest.xml
package: $(APK_DIR)/build.xml
package: $(APK_DIR)/local.properties
package: $(APK_DIR)/project.properties
package: $(APK_DIR)/res/
package: $(APK_DIR)/src/
package: $(APK_DIR)/jni/Android.mk
package: $(APK_DIR)/jni/Application.mk
package: $(APK_DIR)/jni/runtime-bootstrap.c
package: $(patsubst %,$(APK_DIR)/jni/armeabi-v7a/%,$(MONO_LIBS))
package: $(patsubst %,$(APK_DIR)/jni/arm64-v8a/%,$(MONO_LIBS))
package: $(patsubst %,$(APK_DIR)/jni/x86/%,$(MONO_LIBS))
package: $(patsubst %,$(APK_DIR)/jni/x86_64/%,$(MONO_LIBS))
package: $(APK_DIR)/assets/mconfig
package: $(APK_DIR)/assets/asm/main.exe
package: $(APK_DIR)/assets/asm/Mono.Android.dll
package: $(patsubst %,$(APK_DIR)/assets/asm/%,$(BCL_LIBS))
package: $(patsubst %,$(APK_DIR)/assets/asm/%,$(NUNIT_LIBS))
package: $(patsubst %,$(APK_DIR)/assets/asm/monodroid_%_test.dll,$(CHECKS))

.PHONY: build-%
build-%: package
	printf "monodroid_$*_test.dll" > $(APK_DIR)/assets/asm/testassembly.txt
	cd $(APK_DIR) && $(NDK_BUILD)
	cd $(APK_DIR) && $(ANT) debug

.PHONY: check-%
check-%: build-%
	cd $(APK_DIR) && $(ADB) uninstall $(PACKAGE) || true
	cd $(APK_DIR) && $(ADB) install bin/AndroidActivity-debug.apk
	cd $(APK_DIR) && $(ADB) shell am instrument -w $(PACKAGE)/$(RUNNER)

## Mini tests

MINI_TEST_SOURCES= \
	$(TOP)/mono/mini/aot-tests.cs \
	$(TOP)/mono/mini/arrays.cs \
	$(TOP)/mono/mini/basic-calls.cs \
	$(TOP)/mono/mini/basic-float.cs \
	$(TOP)/mono/mini/basic-long.cs \
	$(TOP)/mono/mini/basic-math.cs \
	$(TOP)/mono/mini/basic.cs \
	$(TOP)/mono/mini/exceptions.cs \
	$(TOP)/mono/mini/generics.cs \
	$(TOP)/mono/mini/gshared.cs \
	$(TOP)/mono/mini/objects.cs \
	$(TOP)/mono/mini/TestDriver.cs \
	managed/mini-test-runner.cs

$(APK_DIR)/assets/asm/monodroid_mini_test.dll: $(MINI_TEST_SOURCES)
	mcs /nostdlib /unsafe -target:library -out:$@ -define:__MOBILE__ $(BCL_REFERENCES) $(NUNIT_REFERENCES) $(MINI_TEST_SOURCES)

## BCL tests

##
# $(1): assembly name (corlib, System, etc.)
define TestAssemblyTemplate

.PHONY: $$(TOP)/mcs/class/$(1)/monodroid_$(1)_test.dll
$$(TOP)/mcs/class/$(1)/monodroid_$(1)_test.dll:
	$$(MAKE) PROFILE='monodroid' -C $$(TOP)/mcs/class/$(1) build-test-lib

$(APK_DIR)/assets/asm/monodroid_$(1)_test.dll: $$(TOP)/mcs/class/$(1)/monodroid_$(1)_test.dll
	cp $$< $$@

endef

$(foreach check,$(BCL_CHECKS),$(eval $(call TestAssemblyTemplate,$(check))))
