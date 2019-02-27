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
all: .stamp-package

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

local.properties:
	echo "sdk.dir=$(SDK_DIR)" >  $@
	echo "ndk.dir=$(NDK_DIR)" >> $@

## Directories

app/assets/asm app/src/main/jniLibs/armeabi-v7a app/src/main/jniLibs/arm64-v8a app/src/main/jniLibs/x86 app/src/main/jniLibs/x86_64:
	mkdir -p $@

## Package the monosgen-2.0, MonoPosixHelper and mono-native libraries

MONO_LIBS= \
	libmonosgen-2.0.so \
	libMonoPosixHelper.so \
	libmono-native.so

$(patsubst %,app/src/main/jniLibs/armeabi-v7a/%,$(MONO_LIBS)): app/src/main/jniLibs/armeabi-v7a/%: $(TOP)/sdks/out/android-armeabi-v7a-$(CONFIGURATION)/lib/% | app/src/main/jniLibs/armeabi-v7a
	cp $< $@

.stamp-package: $(patsubst %,app/src/main/jniLibs/armeabi-v7a/%,$(MONO_LIBS))

$(patsubst %,app/src/main/jniLibs/arm64-v8a/%,$(MONO_LIBS)): app/src/main/jniLibs/arm64-v8a/%: $(TOP)/sdks/out/android-arm64-v8a-$(CONFIGURATION)/lib/% | app/src/main/jniLibs/arm64-v8a
	cp $< $@

.stamp-package: $(patsubst %,app/src/main/jniLibs/arm64-v8a/%,$(MONO_LIBS))

$(patsubst %,app/src/main/jniLibs/x86/%,$(MONO_LIBS)): app/src/main/jniLibs/x86/%: $(TOP)/sdks/out/android-x86-$(CONFIGURATION)/lib/% | app/src/main/jniLibs/x86
	cp $< $@

.stamp-package: $(patsubst %,app/src/main/jniLibs/x86/%,$(MONO_LIBS))

$(patsubst %,app/src/main/jniLibs/x86_64/%,$(MONO_LIBS)): app/src/main/jniLibs/x86_64/%: $(TOP)/sdks/out/android-x86_64-$(CONFIGURATION)/lib/% | app/src/main/jniLibs/x86_64
	cp $< $@

.stamp-package: $(patsubst %,app/src/main/jniLibs/x86_64/%,$(MONO_LIBS))

## Package Mono.Android.dll

app/assets/asm/Mono.Android.dll: managed/fake-monodroid.cs | app/assets/asm
	csc /nostdlib /noconfig /langversion:latest /t:library /out:$@ $(patsubst %,-r:$(TOP)/sdks/out/android-bcl/monodroid/%.dll,mscorlib System System.Core) $<

.stamp-package: app/assets/asm/Mono.Android.dll

## Package the BCL

BCL=$(patsubst $(TOP)/sdks/out/android-bcl/monodroid/%,%,$(shell find $(TOP)/sdks/out/android-bcl/monodroid -type f))

$(patsubst %,app/assets/asm/%,$(BCL)): app/assets/asm/%: $(TOP)/sdks/out/android-bcl/monodroid/% | app/assets/asm
	cp -R $(TOP)/sdks/out/android-bcl/monodroid/* app/assets/asm

.stamp-package: $(patsubst %,app/assets/asm/%,$(BCL))

## Run Mono.Debugger.Soft and BCL tests

.stamp-package:
	touch $@

.stamp-install$(if $(MONO_WAIT_LLDB),-waitforlldb): .stamp-package local.properties app/src/main/c/runtime-bootstrap.c app/src/main/java/org/mono/android/AndroidRunner.java
	# Remove the *other* stamp file
	rm -f .stamp-install$(if $(MONO_WAIT_LLDB),,-waitforlldb)
	./gradlew uninstallAll assembleDebug installDebug $(if $(MONO_WAIT_LLDB),-Pmono.waitforlldb)
	touch $@

.PHONY: check-Mono.Debugger.Soft
check-Mono.Debugger.Soft: .stamp-install$(if $(MONO_WAIT_LLDB),-waitforlldb)
	PATH="$$PATH:$(dir $(ADB))" mono --debug $(TOP)/sdks/out/android-bcl/monodroid_tools/nunit-lite-console.exe \
		-labels \
		-exclude:NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,MobileNotWorking,AndroidNotWorking,LargeFileSupport,AndroidSdksNotWorking \
		-remote:android:$(PACKAGE)/$(RUNNER) \
		-result:TestResult-Mono.Debugger.Soft.xml \
		-format:nunit2 \
		$(if $(TESTNAME),-test:$(TESTNAME)) \
		$(TOP)/sdks/out/android-bcl/monodroid_tools/tests/monodroid_tools_Mono.Debugger.Soft_test.dll

.PHONY: check-%
check-%: .stamp-install$(if $(MONO_WAIT_LLDB),-waitforlldb)
	PATH="$$PATH:$(dir $(ADB))" mono --debug $(TOP)/sdks/out/android-bcl/monodroid_tools/nunit-lite-console.exe \
		-labels \
		-exclude:NotOnMac,NotWorking,ValueAdd,CAS,InetAccess,MobileNotWorking,AndroidNotWorking,LargeFileSupport,AndroidSdksNotWorking \
		-android:$(PACKAGE)/$(RUNNER) \
		-result:TestResult-$*.xml \
		-format:nunit2 \
		$(if $(TESTNAME),-test:$(TESTNAME)) \
		$(TOP)/sdks/out/android-bcl/monodroid/tests/monodroid_$*_test.dll

attach-lldb:
	PATH="$$PATH:$(dir $(ADB))" $(TOP)/sdks/android/lldb-android.sh "$(PACKAGE)"
