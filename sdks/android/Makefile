TOP=$(realpath $(CURDIR)/../..)
-include $(TOP)/sdks/Make.config
include $(TOP)/sdks/versions.mk
include $(TOP)/sdks/paths.mk
ADB       = $(SDK_DIR)/platform-tools/adb
ANDROID   = $(SDK_DIR)/tools/android
ANT       = ant
NDK_BUILD = $(NDK_DIR)/ndk-build

PACKAGE   = org.mono.android.AndroidTestRunner
RUNNER    = org.mono.android.AndroidRunner
ACTIVITY  = org.mono.android.AndroidActivity

RELEASE?=true

CONFIGURATION=$(if $(filter $(RELEASE),true),release,debug)

APK_DIR=apk

BCL_RUNNER_EXE = $(APK_DIR)/assets/asm/main.exe
DEBUGGER_RUNNER_EXE = $(APK_DIR)/assets/asm/dtest-app.exe
DEBUGGER_RUNNER_PDB = $(APK_DIR)/assets/asm/dtest-app.pdb


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

BCL_PDBS=$(patsubst %.dll,%.pdb,$(BCL_LIBS))

BCL_REFERENCES=$(patsubst %,-r:$(TOP)/sdks/out/bcl/monodroid/%,$(BCL_LIBS))

NUNIT_LIBS= \
	nunitlite.dll

NUNIT_REFERENCES=$(patsubst %,-r:$(TOP)/sdks/out/bcl/monodroid/%,$(NUNIT_LIBS))

$(APK_DIR) $(APK_DIR)/jni $(APK_DIR)/jni/armeabi $(APK_DIR)/jni/armeabi-v7a $(APK_DIR)/jni/arm64-v8a $(APK_DIR)/jni/x86 $(APK_DIR)/jni/x86_64 $(APK_DIR)/assets $(APK_DIR)/assets/asm:
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

$(patsubst %,$(APK_DIR)/jni/armeabi/%,$(MONO_LIBS)): $(APK_DIR)/jni/armeabi/%: $(TOP)/sdks/out/android-armeabi-$(CONFIGURATION)/lib/% | $(APK_DIR)/jni/armeabi
	cp $< $@

$(patsubst %,$(APK_DIR)/jni/armeabi-v7a/%,$(MONO_LIBS)): $(APK_DIR)/jni/armeabi-v7a/%: $(TOP)/sdks/out/android-armeabi-v7a-$(CONFIGURATION)/lib/% | $(APK_DIR)/jni/armeabi-v7a
	cp $< $@

$(patsubst %,$(APK_DIR)/jni/arm64-v8a/%,$(MONO_LIBS)): $(APK_DIR)/jni/arm64-v8a/%: $(TOP)/sdks/out/android-arm64-v8a-$(CONFIGURATION)/lib/% | $(APK_DIR)/jni/arm64-v8a
	cp $< $@

$(patsubst %,$(APK_DIR)/jni/x86/%,$(MONO_LIBS)): $(APK_DIR)/jni/x86/%: $(TOP)/sdks/out/android-x86-$(CONFIGURATION)/lib/% | $(APK_DIR)/jni/x86
	cp $< $@

$(patsubst %,$(APK_DIR)/jni/x86_64/%,$(MONO_LIBS)): $(APK_DIR)/jni/x86_64/%: $(TOP)/sdks/out/android-x86_64-$(CONFIGURATION)/lib/% | $(APK_DIR)/jni/x86_64
	cp $< $@

$(APK_DIR)/assets/mconfig: machine.config | $(APK_DIR)/assets
	cp $< $@

$(APK_DIR)/assets/asm/main.exe: managed/main.cs | $(APK_DIR)/assets/asm
	mcs /nostdlib -unsafe -out:$@ $(BCL_REFERENCES) $(NUNIT_REFERENCES) $<

$(APK_DIR)/assets/asm/Mono.Android.dll: managed/fake-monodroid.cs | $(APK_DIR)/assets/asm
	mcs /nostdlib -target:library -out:$@ $(BCL_REFERENCES) $<

$(patsubst %,$(APK_DIR)/assets/asm/%,$(BCL_LIBS)) $(patsubst %,$(APK_DIR)/assets/asm/%,$(NUNIT_LIBS)): $(APK_DIR)/assets/asm/%: $(TOP)/sdks/out/bcl/monodroid/% | $(APK_DIR)/assets/asm
	cp $< $@

$(APK_DIR)/lldb-mono-0.1/xa-lldb:
	cd $(APK_DIR) && curl -L -O 'https://github.com/mono/lldb-binaries/releases/download/v0.1/lldb-mono-0.1.tar.gz'
	cd $(APK_DIR) && tar xfv lldb-mono-0.1.tar.gz && rm lldb-mono-0.1.tar.gz
	cd $(APK_DIR) && rm lldb-mono-0.1/xa-lldb
	cd $(APK_DIR) && cp ../xa-lldb lldb-mono-0.1

$(patsubst %,$(APK_DIR)/assets/asm/%,$(BCL_PDBS)): $(APK_DIR)/assets/asm/%: $(TOP)/sdks/out/bcl/monodroid/% | $(APK_DIR)/assets/asm
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
package: $(patsubst %,$(APK_DIR)/jni/armeabi/%,$(MONO_LIBS))
package: $(patsubst %,$(APK_DIR)/jni/armeabi-v7a/%,$(MONO_LIBS))
package: $(patsubst %,$(APK_DIR)/jni/arm64-v8a/%,$(MONO_LIBS))
package: $(patsubst %,$(APK_DIR)/jni/x86/%,$(MONO_LIBS))
package: $(patsubst %,$(APK_DIR)/jni/x86_64/%,$(MONO_LIBS))
package: $(APK_DIR)/assets/mconfig
package: $(APK_DIR)/assets/asm/Mono.Android.dll
package: $(patsubst %,$(APK_DIR)/assets/asm/%,$(BCL_LIBS))
package: $(patsubst %,$(APK_DIR)/assets/asm/%,$(BCL_PDBS))
package: $(patsubst %,$(APK_DIR)/assets/asm/%,$(NUNIT_LIBS))
package: $(patsubst %,$(APK_DIR)/assets/asm/monodroid_%_test.dll,$(CHECKS))
ifneq ($(MONO_WAIT_LLDB),)
package: $(APK_DIR)/lldb-mono-0.1/xa-lldb
endif

package-bcl: package
	rm -rf $(APK_DIR)/bin/AndroidActivity-debug.apk
	rm -rf $(DEBUGGER_RUNNER_EXE)
	$(MAKE) $(BCL_RUNNER_EXE)

package-debugger: package
	rm -rf $(BCL_RUNNER_EXE)
	rm -rf $(APK_DIR)/bin/AndroidActivity-debug.apk
	rm -rf $(APK_DIR)/assets/asm/testassembly.txt
	$(MAKE) $(DEBUGGER_RUNNER_EXE)
	$(MAKE) $(DEBUGGER_RUNNER_PDB)

.PHONY: $(TOP)/mcs/class/Mono.Debugger.Soft/dtest-app.exe $(TOP)/mcs/class/Mono.Debugger.Soft/dtest-app.pdb
$(TOP)/mcs/class/Mono.Debugger.Soft/dtest-app.exe $(TOP)/mcs/class/Mono.Debugger.Soft/dtest-app.pdb:
	$(MAKE) PROFILE='monodroid' -C $(TOP)/mcs/class/Mono.Debugger.Soft dtest-app.exe

$(APK_DIR)/assets/asm/dtest-app.exe: $(TOP)/mcs/class/Mono.Debugger.Soft/dtest-app.exe
	cp $< $@

$(APK_DIR)/assets/asm/dtest-app.pdb: $(TOP)/mcs/class/Mono.Debugger.Soft/dtest-app.pdb
	cp $< $@

make-debugger-test: package-debugger
	cd $(APK_DIR) && RUN_WITH_MANAGED_DEBUGGER=1 MONO_DEBUGGER_TESTS=1 $(NDK_BUILD)
	cd $(APK_DIR) && $(ANT) debug

# fixme:
# eventual api:
# MONO_EXE=foo.exe make run-debug-managed-droid
# MONO_EXE=foo.exe make run-debug-unmanaged-droid
#
# mono.debugger tests can just run those targets in the setup function
run-debugger-test: make-debugger-test
	cd $(APK_DIR) && $(ADB) uninstall $(PACKAGE) || true
	cd $(APK_DIR) && $(ADB) install bin/AndroidActivity-debug.apk
	$(ADB) reverse tcp:6100 tcp:6100
	cd $(APK_DIR) && $(ADB) shell am instrument $(PACKAGE)/$(RUNNER)

# Skip rerunning build steps / cleanup steps
# And run unattended
.PHONY: dirty-run-debugger-test
dirty-run-debugger-test:
	$(ADB) shell am instrument $(PACKAGE)/$(RUNNER)

.PHONY: run-debugger-test-cleanup
run-debugger-test-cleanup:
	$(ADB) reverse --remove-all

.PHONY: build-%
build-%: package-bcl
	printf "monodroid_$*_test.dll" > $(APK_DIR)/assets/asm/testassembly.txt
	cd $(APK_DIR) && MONO_BCL_TESTS=1 $(NDK_BUILD)
	cd $(APK_DIR) && $(ANT) debug

.PHONY: check-%
check-%: build-%
	cd $(APK_DIR) && $(ADB) uninstall $(PACKAGE) || true
	cd $(APK_DIR) && $(ADB) install bin/AndroidActivity-debug.apk
ifneq ($(MONO_WAIT_LLDB),)
	cd $(APK_DIR) && ./lldb-mono-0.1/xa-lldb
else
	cd $(APK_DIR) && $(ADB) shell am instrument -w $(PACKAGE)/$(RUNNER)
endif

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

.PHONY: $$(TOP)/mcs/class/lib/monodroid/tests/monodroid_$(1)_test.dll
$$(TOP)/mcs/class/lib/monodroid/tests/monodroid_$(1)_test.dll:
	$$(MAKE) PROFILE='monodroid' -C $$(TOP)/mcs/class/$(1) build-test-lib

$(APK_DIR)/assets/asm/monodroid_$(1)_test.dll: $$(TOP)/mcs/class/lib/monodroid/tests/monodroid_$(1)_test.dll
	cp $$< $$@

endef

$(foreach check,$(BCL_CHECKS),$(eval $(call TestAssemblyTemplate,$(check))))
