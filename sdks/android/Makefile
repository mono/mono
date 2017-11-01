TOP=$(realpath $(CURDIR)/../..)
include $(TOP)/sdks/paths.mk
ADB       = $(SDK_DIR)/platform-tools/adb
ANDROID   = $(SDK_DIR)/tools/android
ANT       = ant
NDK_BUILD = $(NDK_DIR)/ndk-build

PACKAGE   = org.mono.android.AndroidTestRunner
ACTIVITY  = org.mono.android.AndroidRunner

PROJECT_SETUP_FILES = project.properties local.properties


.PHONY: all
all: setup stage-sdk
	$(MAKE) -C managed all
	$(NDK_BUILD)
	$(ANT) debug

$(PROJECT_SETUP_FILES): build.xml AndroidManifest.xml
	$(ANDROID) update project -p . -t "android-14"

setup: $(PROJECT_SETUP_FILES)

jni/armeabi-v7a jni/arm64-v8a jni/x86 jni/x86_64 assets:
	mkdir -p $@

MONO_LIBS=libMonoPosixHelper.so libmonosgen-2.0.so

$(foreach lib,$(MONO_LIBS),jni/armeabi-v7a/$(lib)): jni/armeabi-v7a/%: $(TOP)/sdks/out/android-arm/lib/% jni/armeabi-v7a
	cp $(TOP)/sdks/out/android-arm/lib/$* $@

$(foreach lib,$(MONO_LIBS),jni/arm64-v8a/$(lib)): jni/arm64-v8a/%: $(TOP)/sdks/out/android-arm64/lib/% jni/arm64-v8a
	cp $(TOP)/sdks/out/android-arm64/lib/$* $@

$(foreach lib,$(MONO_LIBS),jni/x86/$(lib)): jni/x86/%: $(TOP)/sdks/out/android-x86/lib/% jni/x86
	cp $(TOP)/sdks/out/android-x86/lib/$* $@

$(foreach lib,$(MONO_LIBS),jni/x86_64/$(lib)): jni/x86_64/%: $(TOP)/sdks/out/android-x86_64/lib/% jni/x86_64
	cp $(TOP)/sdks/out/android-x86_64/lib/$* $@

assets/mconfig: machine.config assets
	cp machine.config assets/mconfig

stage-sdk: $(foreach lib,$(MONO_LIBS),jni/armeabi-v7a/$(lib)) $(foreach lib,$(MONO_LIBS),jni/arm64-v8a/$(lib)) \
	$(foreach lib,$(MONO_LIBS),jni/x86/$(lib)) $(foreach lib,$(MONO_LIBS),jni/x86_64/$(lib)) assets/mconfig

#This is hilarious, but we can't clean unless we got the 2 property files around
.PHONY: clean
clean: $(PROJECT_SETUP_FILES)
	$(ANT) clean
	rm $(PROJECT_SETUP_FILES)

.PHONY: deploy
deploy: all
	$(ADB) install -r bin/AndroidRunner-debug.apk

.PHONY: undeploy
undeploy:
	$(ADB) uninstall $(PACKAGE)

.PHONY: redeploy
redeploy:
	$(MAKE) undeploy
	$(MAKE) deploy

.PHONY:  run
run:
	$(ADB) shell am start -a android.intent.action.MAIN -c android.intent.category.LAUNCHER $(PACKAGE)/$(ACTIVITY)

.PHONY: kill
kill:
	$(ADB) shell am force-stop $(PACKAGE)

.PHONY: logcat
logcat:
	$(ADB) logcat

.PHONY: shell
shell:
	$(ADB) shell
