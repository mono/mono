TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk

UNAME=$(shell uname)

SDK_DIR = $(ANDROID_TOOLCHAIN_DIR)/sdk
NDK_DIR = $(ANDROID_TOOLCHAIN_DIR)/ndk

ADB       = $(SDK_DIR)/platform-tools/adb

PACKAGE   = org.mono.android.AndroidTestRunner
RUNNER    = org.mono.android.AndroidRunner
ACTIVITY  = org.mono.android.AndroidActivity

CONFIGURATION?=release

APK_DIR=apk

DEBUGGER_RUNNER_EXE = app/assets/asm/dtest-app.exe
DEBUGGER_RUNNER_PDB = app/assets/asm/dtest-app.pdb

CHECKS=$(subst mscorlib,corlib,$(patsubst $(TOP)/sdks/out/android-bcl/monodroid/tests/monodroid_%_test.dll,%,$(wildcard $(TOP)/sdks/out/android-bcl/monodroid/tests/monodroid_*_test.dll)))

.PHONY: all
all: package

#This is hilarious, but we can't clean unless we got the 2 property files around
.PHONY: clean
clean:
	./gradlew clean

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

## Accept Android License

.PHONY: accept-android-license
accept-android-license:
	yes | $(SDK_DIR)/tools/bin/sdkmanager --licenses

## Check targets

BCL_LIBS=$(patsubst %,%.dll,$(filter-out Mono.Runtime.Tests,$(subst corlib,mscorlib,$(CHECKS))))
BCL_PDBS=$(patsubst %,%.pdb,$(filter-out Mono.Runtime.Tests,$(subst corlib,mscorlib,$(CHECKS))))
BCL_REFERENCES=$(patsubst %,-r:$(TOP)/sdks/out/android-bcl/monodroid/%,$(BCL_LIBS))

NUNIT_LIBS= \
	nunitlite.dll

NUNIT_REFERENCES=$(patsubst %,-r:$(TOP)/sdks/out/android-bcl/monodroid/%,$(NUNIT_LIBS))

local.properties:
	echo "sdk.dir=$(SDK_DIR)" >  $@
	echo "ndk.dir=$(NDK_DIR)" >> $@

app/assets/asm:
	mkdir -p $@

## Package the monosgen-2.0, MonoPosixHelper and mono-native libraries

MONO_LIBS= \
	libmonosgen-2.0.so \
	libMonoPosixHelper.so \
	libmono-native.so

##
# Parameters:
#  $(1): arch
define RuntimeTemplate
app/src/main/jniLibs/$(1):
	mkdir -p $$@

$$(patsubst %,app/src/main/jniLibs/$(1)/%,$$(MONO_LIBS)): app/src/main/jniLibs/$(1)/%: $$(TOP)/sdks/out/android-$(1)-$$(CONFIGURATION)/lib/% | app/src/main/jniLibs/$(1)
	cp $$< $$@

package: $$(patsubst %,app/src/main/jniLibs/$(1)/%,$$(MONO_LIBS))
endef

$(eval $(call RuntimeTemplate,armeabi-v7a))
$(eval $(call RuntimeTemplate,arm64-v8a))
$(eval $(call RuntimeTemplate,x86))
$(eval $(call RuntimeTemplate,x86_64))

## Package Mono.Android.dll

app/assets/asm/Mono.Android.dll: managed/fake-monodroid.cs | app/assets/asm
	csc /nostdlib /noconfig /langversion:latest /t:library /out:$@ $(BCL_REFERENCES) $<

package: app/assets/asm/Mono.Android.dll

## Package the BCL assemblies

$(patsubst %,app/assets/asm/%,$(BCL_LIBS)) $(patsubst %,app/assets/asm/%,$(BCL_PDBS)) $(patsubst %,app/assets/asm/%,$(NUNIT_LIBS)): app/assets/asm/%: $(TOP)/sdks/out/android-bcl/monodroid/% | app/assets/asm
	cp $< $@

package: $(patsubst %,app/assets/asm/%,$(BCL_LIBS)) $(patsubst %,app/assets/asm/%,$(BCL_PDBS)) $(patsubst %,app/assets/asm/%,$(NUNIT_LIBS))

## Package the BCL test assemblies
##
# Parameters:
#  $(1): assembly name (corlib, System, etc.)
define BCLTestAssemblyTemplate
app/assets/asm/monodroid_$(1)_test.dll: $$(TOP)/sdks/out/android-bcl/monodroid/tests/monodroid_$(1)_test.dll | app/assets/asm
	cp $$< $$@

app/assets/asm/monodroid_$(1)_test.dll.config: $$(TOP)/sdks/out/android-bcl/monodroid/tests/monodroid_$(1)_test.dll.nunitlite.config | app/assets/asm
	cp $$< $$@

package: app/assets/asm/monodroid_$(1)_test.dll app/assets/asm/monodroid_$(1)_test.dll.config
endef

$(foreach check,$(CHECKS),$(eval $(call BCLTestAssemblyTemplate,$(check))))

# corlib tests need additional resource assemblies
app/assets/asm/monodroid_corlib_test.dll: app/assets/asm/es-ES/monodroid_corlib_test.resources.dll app/assets/asm/nn-NO/monodroid_corlib_test.resources.dll

app/assets/asm/%/monodroid_corlib_test.resources.dll: $(TOP)/sdks/out/android-bcl/monodroid/tests/%/monodroid_corlib_test.resources.dll
	mkdir -p $(dir $@)
	cp $< $@

## Package Mono.Debugger.Soft test assemblies

.PHONY: $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.exe $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.pdb
$(TOP)/mcs/class/lib/monodroid/tests/dtest-app.exe $(TOP)/mcs/class/lib/monodroid/tests/dtest-app.pdb:
	$(MAKE) PROFILE='monodroid' -C $(TOP)/mcs/class/Mono.Debugger.Soft build-dtest

app/assets/asm/dtest-app.exe app/assets/asm/dtest-app.pdb: app/assets/asm/%: $(TOP)/mcs/class/lib/monodroid/tests/% | app/assets/asm
	cp $< $@

package: app/assets/asm/dtest-app.exe app/assets/asm/dtest-app.pdb

## LLDB support

lldb-mono-0.1/xa-lldb: xa-lldb
	wget --no-verbose -O - 'https://github.com/mono/lldb-binaries/releases/download/v0.1/lldb-mono-0.1.tar.gz' | tar xvf -
	cp xa-lldb lldb-mono-0.1/xa-lldb

## Run Mono.Debugger.Soft and BCL tests

.PHONY: build-Mono.Debugger.Soft
build-Mono.Debugger.Soft: package local.properties
	./gradlew uninstallAll assembleDebug installDebug -Pmono.debuggertests $(if $(MONO_WAIT_LLDB),-Pmono.waitforlldb)

.PHONY: check-Mono.Debugger.Soft
check-Mono.Debugger.Soft: build-Mono.Debugger.Soft $(if $(MONO_WAIT_LLDB),lldb-mono-0.1/xa-lldb)
	$(ADB) reverse tcp:6100 tcp:6100
	$(if $(MONO_WAIT_LLDB),lldb-mono-0.1/xa-lldb,$(ADB) shell am instrument -w $(PACKAGE)/$(RUNNER))

.PHONY: build-BCL
build-BCL: package local.properties
	./gradlew uninstallAll assembleDebug installDebug -Pmono.bcltests $(if $(MONO_WAIT_LLDB),-Pmono.waitforlldb)

.PHONY: check-%
check-%: build-BCL $(if $(MONO_WAIT_LLDB),lldb-mono-0.1/xa-lldb)
	PATH="$$PATH:$(dir $(ADB))" mono $(TOP)/sdks/out/android-bcl/monodroid_tools/nunit-lite-console.exe -labels -android:$(PACKAGE)/$(RUNNER) -result:TestResult-$*.xml -format:nunit2 $(TOP)/sdks/out/android-bcl/monodroid/tests/monodroid_$*_test.dll
